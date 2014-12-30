using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Alphicsh.Audio.Analysis;
using Alphicsh.Audio.Analysis.WaveMatching;
using Alphicsh.Audio.Streaming;
using Alphicsh.Interfaces.Processes;

namespace Alphicsh.Audio.Analysis.LoopDetection
{
    /// <summary>
    /// A function creating a loop detection process.
    /// </summary>
    /// <param name="path">The path of the audio file.</param>
    /// <param name="parameters">The parameters of loop detection algorithm.</param>
    /// <returns>A loop detection process to run.</returns>
    public delegate ILoopDetectionProcess LoopDetectionBlueprint(String path, IDictionary<String, Object> parameters);

    /// <summary>
    /// A class for Sysaldis-based loop detection algorithm, implemented as multiton.
    /// </summary>
    public class SysaldisLoopDetection : ILoopDetectionAlgorithm
    {
        //TO DO: clean all that mess here, somehow

        #region Common

        public String Name { get; private set; }
        private LoopDetectionBlueprint Blueprint;

        /// <summary>
        /// Declares a new loop detection algorithm with a given name and setup.
        /// </summary>
        /// <param name="name">The name of the algorithm.</param>
        /// <param name="blueprint">The process blueprint.</param>
        private SysaldisLoopDetection(String name, LoopDetectionBlueprint blueprint)
        {
            this.Name = name;
            this.Blueprint = blueprint;
        }

        public ILoopDetectionProcess Search(String path, IDictionary<String, Object> parameters)
        {
            return Blueprint(path, parameters);
        }

        /// <summary>
        /// Passes the initial part of loop detection pipeline that reads samples into array of shorts.
        /// </summary>
        /// <param name="path">The path of an audio file.</param>
        /// <param name="parameters">The loop detection algorithm parameters.</param>
        /// <returns>An initial fragment of the loop detection pipeline.</returns>
        private static ProcessPipelineBuilder<ITrackingExtendedProcess, SamplesReader> ReadSamples(String path, IDictionary<String, Object> parameters)
        {
            return ProcessPipelineBuilding
                .Init<ITrackingExtendedProcess, MemoryWaveStreamBuilder>(() => new MemoryWaveStreamBuilder(path))
                .Pipe((previous) => new SamplesReader(previous.Result));
        }

        /// <summary>
        /// Passes the initial part of loop detection pipeline that reads samples into array of shorts and cuts the fade-out at the end.
        /// </summary>
        /// <param name="path">The path of an audio file.</param>
        /// <param name="parameters">The loop detection algorithm parameters.</param>
        /// <returns>An initial fragment of the loop detection pipeline.</returns>
        private static ProcessPipelineBuilder<ITrackingExtendedProcess, SamplesTrimmer> ReadAndTrimSamples(String path, IDictionary<String, Object> parameters)
        {
            return ReadSamples(path, parameters).Pipe((previous) => new SamplesTrimmer(previous.Result, previous.Format));
        }

        /// <summary>
        /// Adds Sysaldis process to finish the loop detection pipeline.
        /// </summary>
        /// <param name="currentPipeline">The pipeline built so far.</param>
        /// <returns>A complete loop detection process.</returns>
        private static CompoundLoopDetectionProcess TackSysaldis(ProcessPipelineBuilder<ITrackingExtendedProcess, QueryPlanner> currentPipeline)
        {
            return new CompoundLoopDetectionProcess(currentPipeline.Pipe((previous) => new SysaldisProcess(previous.Queries)).PassTransforms());
        }

        #endregion

        #region Basic Sysaldis

        /// <summary>
        /// Passes the basic Sysaldis algorithm.
        /// </summary>
        public static SysaldisLoopDetection BasicSysaldis
        {
            get
            {
                if (_BasicSysaldis == null)
                {
                    _BasicSysaldis = new SysaldisLoopDetection("BasicSysaldis", (path, parameters) => TackSysaldis(CreateQueries(path, parameters, false)));
                }
                return _BasicSysaldis;
            }
        }
        private static SysaldisLoopDetection _BasicSysaldis;

        /// <summary>
        /// Passes the Sysaldis algorithm with fade-out trimmed.
        /// </summary>
        public static SysaldisLoopDetection TrimmedSysaldis
        {
            get
            {
                if (_TrimmedSysaldis == null)
                {
                    _TrimmedSysaldis = new SysaldisLoopDetection("TrimmedSysaldis", (path, parameters) => TackSysaldis(CreateQueries(path, parameters, true)));
                }
                return _TrimmedSysaldis;
            }
        }
        private static SysaldisLoopDetection _TrimmedSysaldis;

        /// <summary>
        /// Plans queries using samples read and algorithm parameters provided.
        /// </summary>
        /// <param name="path">The path of the audio file.</param>
        /// <param name="parameters">The algorithm parameters given.</param>
        /// <param name="trim">Whether fade-out should be trimmed or not.</param>
        /// <returns>A loop detection pipeline fragment.</returns>
        private static ProcessPipelineBuilder<ITrackingExtendedProcess, QueryPlanner> CreateQueries(String path, IDictionary<String, Object> parameters, Boolean trim)
        {
            Func<ITrackingExtendedProcess, QueryPlanner> f = (previous) => _CreateQueries(previous,
                    Convert.ToDouble(parameters["refStart"]), Convert.ToDouble(parameters["refEnd"]),
                    Convert.ToDouble(parameters["offMin"]), Convert.ToDouble(parameters["offMax"]),
                    parameters["matches"].ToString().Split(new String[] {" "}, StringSplitOptions.RemoveEmptyEntries));
            if (trim)
            {
                return ReadAndTrimSamples(path, parameters).Pipe(f);
            }
            else
            {
                return ReadSamples(path, parameters).Pipe(f);
            }
        }
        //static method actually creating query planner, based on fraction reference points/loop offsets
        private static QueryPlanner _CreateQueries(ITrackingExtendedProcess previous,
            Double referenceStart, Double referenceEnd,
            Double offsetMin, Double offsetMax,
            IEnumerable<String> matches)
        {
            Int32 length = -1;
            if (previous is SamplesReader)
            {
                length = (previous as SamplesReader).Result[0].Length;
            }
            if (previous is SamplesTrimmer)
            {
                length = (previous as SamplesTrimmer).Result[0].Length;
            }

            Console.WriteLine(length);

            if (length == -1)
            {
                throw new InvalidOperationException("Couldn't retrieve channel data from process " + previous.Name + ".");
            }

            Int32 refMin = (Int32)Math.Round(referenceStart * length);
            Int32 refMax = (Int32)Math.Round(referenceEnd * length);
            Int32 offMin = (Int32)Math.Round(offsetMin * length);
            Int32 offMax = (Int32)Math.Round(offsetMax * length);

            return _CreateQueries(previous, refMin, refMax, offMin, offMax, matches);
        }

        //static method actually creating query planner, based on integer reference points/loop offsets
        private static QueryPlanner _CreateQueries(ITrackingExtendedProcess previous,
            Int32 referenceStart, Int32 referenceEnd,
            Int32 offsetMin, Int32 offsetMax,
            IEnumerable<String> matches)
        {
            var matchers = _MakeMatchers(previous);

            var result = new QueryPlanner("Calculating positions", matchers, referenceStart, referenceEnd, offsetMin, offsetMax, 1, 1);
            _AddQueriesSizes(result, matches);

            return result;
        }

        //creates wave pattern matchers, based on samples obtained from the previous process
        private static IEnumerable<WavePatternMatcher> _MakeMatchers(ITrackingExtendedProcess previous)
        {
            IEnumerable<WavePatternMatcher> matchers = null;
            if (previous is SamplesReader)
            {
                matchers = (previous as SamplesReader).Result.Select(channel => new WavePatternMatcher(channel));
            }
            if (previous is SamplesTrimmer)
            {
                matchers = (previous as SamplesTrimmer).Result.Select(channel => new WavePatternMatcher(channel));
            }

            if (matchers == null)
            {
                throw new InvalidOperationException("Couldn't retrieve channel data from process " + previous.Name + ".");
            }

            return matchers;
        }

        //applies matches iteration sequence to query planner, or something
        private static void _AddQueriesSizes(QueryPlanner planner, IEnumerable<String> matches)
        {
            foreach (var qMatch in matches)
            {
                Boolean mono = qMatch.EndsWith("m");

                var tMatch = mono ? qMatch.Substring(0, qMatch.Length - 1) : qMatch;
                Int32 mCount = Convert.ToInt32(qMatch.Substring(0, qMatch.IndexOf("x")));
                Int32 mLength = Convert.ToInt32(qMatch.Substring(qMatch.IndexOf("x")+1));

                planner.AddQuery(mCount, mLength, mono);
            }
        }

        #endregion

        #region OneLoop

        /// <summary>
        /// Passes the Sysaldis algorithm for single loops.
        /// </summary>
        public static SysaldisLoopDetection OneLoop
        {
            get
            {
                if (_OneLoop == null)
                {
                    _OneLoop = new SysaldisLoopDetection("OneLoop", (path, parameters) => TackSysaldis(CreateOneLoopQueries(path, parameters)));
                }
                return _OneLoop;
            }
        }
        private static SysaldisLoopDetection _OneLoop;

        //creates single loop queries, based on samples received
        private static ProcessPipelineBuilder<ITrackingExtendedProcess, QueryPlanner> CreateOneLoopQueries(String path, IDictionary<String, Object> parameters)
        {
            Func<SamplesTrimmer, QueryPlanner> f = (previous) =>
            {
                var refStart = 3 * previous.Format.SampleRate;
                var refLen = 5 * previous.Format.SampleRate;
                var minOff = Math.Max(refLen, previous.Result[0].Length / 2);
                return _CreateQueries(previous,
                    refStart, refStart + refLen,
                    Math.Max(minOff, previous.Result[0].Length - refStart - refLen - 30*previous.Format.SampleRate),
                    Math.Max(minOff, previous.Result[0].Length - refStart - refLen - 5),
                    parameters["matches"].ToString().Split(new String[] { " " }, StringSplitOptions.RemoveEmptyEntries));
            };
            return ReadAndTrimSamples(path, parameters).Pipe(f);
        }

        #endregion

        #region Backtrack

        /// <summary>
        /// Passes the backtracking Sysaldis algorithm for single loops.
        /// </summary>
        public static SysaldisLoopDetection Backtrack
        {
            get
            {
                if (_Backtrack == null)
                {
                    _Backtrack = new SysaldisLoopDetection("Backtrack", (path, parameters) => TackSysaldis(CreateBacktrackQueries(path, parameters)));
                }
                return _Backtrack;
            }
        }
        private static SysaldisLoopDetection _Backtrack;

        //creates backtracking loop queries, based on samples received
        private static ProcessPipelineBuilder<ITrackingExtendedProcess, QueryPlanner> CreateBacktrackQueries(String path, IDictionary<String, Object> parameters)
        {
            Func<SamplesTrimmer, QueryPlanner> f = (previous) =>
            {
                var refStart = previous.Result[0].Length - 8 * previous.Format.SampleRate;
                return _CreateQueries(previous,
                    refStart, refStart + 5 * previous.Format.SampleRate,
                    -refStart, -refStart + 30 * previous.Format.SampleRate,
                    parameters["matches"].ToString().Split(new String[] { " " }, StringSplitOptions.RemoveEmptyEntries));
            };
            return ReadAndTrimSamples(path, parameters).Pipe(f);
        }

        #endregion

        #region Echoed Backtrack

        /// <summary>
        /// Passes the backtracking Sysaldis algorithm for single loops.
        /// </summary>
        public static SysaldisLoopDetection EchoedBacktrack
        {
            get
            {
                if (_EchoedBacktrack == null)
                {
                    _EchoedBacktrack = new SysaldisLoopDetection("EchoedBacktrack", (path, parameters) => TackSysaldis(CreateEchoedBacktrackQueries(path, parameters)));
                }
                return _EchoedBacktrack;
            }
        }
        private static SysaldisLoopDetection _EchoedBacktrack;

        //creates backtracking loop queries, based on samples received
        private static ProcessPipelineBuilder<ITrackingExtendedProcess, QueryPlanner> CreateEchoedBacktrackQueries(String path, IDictionary<String, Object> parameters)
        {
            Func<SamplesTrimmer, QueryPlanner> f = (previous) =>
            {
                var refStart = previous.Result[0].Length - 8 * previous.Format.SampleRate;
                var planner = _CreateQueries(previous,
                    refStart, refStart + 5 * previous.Format.SampleRate,
                    -refStart / 2, -5 * previous.Format.SampleRate,
                    parameters["matches"].ToString().Split(new String[] { " " }, StringSplitOptions.RemoveEmptyEntries)
                    );
                    
                foreach (var query in planner.Queries)
                {
                    query.Echoes = new List<Int32>() {1, 2};
                }

                return planner;
            };
            return ReadAndTrimSamples(path, parameters).Pipe(f);
        }

        #endregion

    }
}
