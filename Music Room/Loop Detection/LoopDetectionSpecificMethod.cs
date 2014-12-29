using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Alphicsh.Audio.Analysis.LoopDetection;

namespace Music_Room_Application.Loop_Detection
{
    /// <summary>
    /// A class containing loop detection algorithm and its parameters.
    /// </summary>
    public class LoopDetectionSpecificMethod
    {
        /// <summary>
        /// The name of specific algorithm setup.
        /// </summary>
        public String Name { get; private set; }

        /// <summary>
        /// The base loop detection algorithm.
        /// </summary>
        public ILoopDetectionAlgorithm Algorithm { get; private set; }

        /// <summary>
        /// The algorithm parameters.
        /// </summary>
        public IDictionary<String, Object> Parameters { get; private set; }

        /// <summary>
        /// Creates a new specific method with a name, algorithm and parameters.
        /// </summary>
        /// <param name="name">The name of specific method.</param>
        /// <param name="algorithm">The base algorithm.</param>
        /// <param name="parameters">The algorithm parameters.</param>
        public LoopDetectionSpecificMethod(String name, ILoopDetectionAlgorithm algorithm, IDictionary<String, Object> parameters)
        {
            this.Name = name;
            this.Algorithm = algorithm;
            this.Parameters = new Dictionary<String, Object>(parameters);
        }

        /// <summary>
        /// Creates a loop detection process for a given file.
        /// </summary>
        /// <param name="path">The path of file with audio data.</param>
        /// <returns>A loop detection process to run.</returns>
        public ILoopDetectionProcess Search(String path)
        {
            return this.Algorithm.Search(path, this.Parameters);
        }

        /// <summary>
        /// Creates a loop detection process for a given track.
        /// </summary>
        /// <param name="track">The track to detect audio in.</param>
        /// <returns>A loop detection process to run.</returns>
        public ILoopDetectionProcess Search(TrackInfo track)
        {
            //creating base process
            var process = this.Search(track.Path);

            //preparing process so that it updates track's loop data on completion
            process.ProcessCompleted += (sender, e) => {
                var loopProcess = sender as ILoopDetectionProcess;
                track.LoopStart = loopProcess.LoopStart;
                track.LoopLength = loopProcess.LoopLength;
                track.LoopState = track.LoopLength + " from " + track.LoopStart;
            };

            //passing the process
            return process;
        }
    }
}
