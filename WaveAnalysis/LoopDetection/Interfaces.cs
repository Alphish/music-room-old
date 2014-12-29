using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Alphicsh.Interfaces.Processes;

namespace Alphicsh.Audio.Analysis.LoopDetection
{
    /// <summary>
    /// An interface for loop detection process.
    /// </summary>
    public interface ILoopDetectionProcess : IExtendedProcess
    {
        /// <summary>
        /// Gets the loop starting point detected, given as sample index.
        /// </summary>
        Int32 LoopStart { get; }
        /// <summary>
        /// Gets the loop length detected, given as number of samples.
        /// </summary>
        Int32 LoopLength { get; }

        /// <summary>
        /// Gets the estimated loop detection accuracy; the higher it is, the higher chances that the correct loop has been found.
        /// </summary>
        Double EstimatedAccuracy { get; }
    }

    /// <summary>
    /// An interface for loop detection algorithm.
    /// </summary>
    public interface ILoopDetectionAlgorithm
    {
        /// <summary>
        /// A name of the algorithm.
        /// </summary>
        String Name { get; }

        /// <summary>
        /// Creates a new loop detection process for a given track.
        /// </summary>
        /// <param name="path">The path of a file to search loop for.</param>
        /// <param name="parameters">The parameters of the algorithm.</param>
        /// <returns>A loop detection process to run.</returns>
        ILoopDetectionProcess Search(String path, IDictionary<String, Object> parameters);
    }

}
