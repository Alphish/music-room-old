using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Alphicsh.Interfaces.Processes;
using Alphicsh.Audio.Analysis.WaveMatching;

namespace Alphicsh.Audio.Analysis.LoopDetection
{
    /// <summary>
    /// A class for Stupid Yet Sufficient Algorithm for Loop Detection In Samples process.
    /// </summary>
    public class SysaldisProcess : RepeatedProcess<WaveMatchingProcess>, ILoopDetectionProcess
    {
        public Int32 LoopStart { get; protected set; }
        public Int32 LoopLength { get; protected set; }

        public Double EstimatedAccuracy { get; protected set; }

        //the queries for Sysaldis iterations
        private List<WaveMatchingQuery> Queries;

        public SysaldisProcess(IEnumerable<WaveMatchingQuery> queries)
            : base("SYSALDIS")
        {
            this.Queries = new List<WaveMatchingQuery>(queries);
            this.Target = Queries.Count;

            this.LoopStart = -1;
            this.LoopLength = -1;
            this.EstimatedAccuracy = 0;
        }

        //creates a new iteration
        protected override WaveMatchingProcess PassNextProcess()
        {
            var procIdx = (Int32)this.Progress;
            var query = (procIdx < Queries.Count) ? Queries[procIdx] : null;

            if (Subprocess != null)
            {
                this._PrepareNextQuery(query, Subprocess);
            }
            if (query == null)
            {
                return null;
            }

            return new WaveMatchingProcess(
                "substep no. "+(procIdx+1)+"/"+Target+ " (" + query.ReferencePoints.Count() +"x" + query.MatchLength +")",
                query);
        }

        //gathers results from the previous SYSALDIS iteration and passes the most likely candidates to the next iteration
        private void _PrepareNextQuery(WaveMatchingQuery query, WaveMatchingProcess previousProcess)
        {
            var results = previousProcess.Results;

            //finds best AWSM value, and calculates the overall result if it's the last iteration
            var max = results.Values.Max();
            if (query == null)
            {
                this._BuildResult(Queries.Last(), results.First(result => result.Value == max).Key);
                return;
            }

            //calculates some statistics about overall AWSM to determine AWSM threshold
            var avg = results.Values.Average();
            var maxPerCent = (Double)max / previousProcess.ScoreCap;
            var threshold = (1 - maxPerCent) * avg + maxPerCent * (0.9 * max);
            threshold = Math.Min(0.9 * max, threshold);

            //removes all candidates with AWSM below threshold, and pass remaining ones to the next query
            var toInject = results.Where(kvp => kvp.Value >= threshold).Select(kvp => kvp.Key).ToList();
            query.InjectOffsets(toInject);
        }

        //determines the loop parameters and estimated accuracy
        private void _BuildResult(WaveMatchingQuery query, Int32 length)
        {
            this.LoopLength = length;

            //finds the best looking loop starting point
            var refPointsRange = (query.ReferencePoints.Last() + query.MatchLength) - query.ReferencePoints.First();
            var matchLength = refPointsRange / query.ReferencePoints.Count();
            var startMatches = query.ReferencePoints.ToDictionary(point => point, point => query.Matchers.Sum(
                matcher => matcher.CompareWaves(point, LoopLength, matchLength, query.StrictScore, query.LooseScore
                )));
            var max = startMatches.Max(kvp => kvp.Value);

            this.LoopStart = startMatches.First(kvp => kvp.Value == max).Key;

            //handles the backtracking, i.e. when loop point is detected earlier than loop start
            if (this.LoopLength < 0)
            {
                this.LoopStart += this.LoopLength;
                this.LoopLength *= -1;
            }

            //estimates the accuracy
            this.EstimatedAccuracy = 100 * ((Double)max / (matchLength * (query.StrictScore + query.LooseScore) * query.Matchers.Count()));
        }
    }
}
