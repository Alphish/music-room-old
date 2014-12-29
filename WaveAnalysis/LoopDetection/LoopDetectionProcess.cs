using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Alphicsh.Interfaces.Processes;

namespace Alphicsh.Audio.Analysis.LoopDetection
{
    /// <summary>
    /// A class for multiple step loop detection process. The last step is assumed to be a loop detection process, too.
    /// </summary>
    public class CompoundLoopDetectionProcess : ProcessPipeline<ITrackingExtendedProcess>, ILoopDetectionProcess
    {
        public Int32 LoopStart { get; protected set; }
        public Int32 LoopLength { get; protected set; }
        public Double EstimatedAccuracy { get; protected set; }

        /// <summary>
        /// Creates a new loop detection pipeline.
        /// </summary>
        /// <param name="name">Name of the process.</param>
        /// <param name="transforms">Fragments of the pipeline.</param>
        public CompoundLoopDetectionProcess(String name, IEnumerable<Func<ITrackingExtendedProcess, ITrackingExtendedProcess>> transforms)
            : base(name, transforms)
        {
            this.LoopStart = -1;
            this.LoopLength = -1;
            this.EstimatedAccuracy = 0;
        }

        /// <summary>
        /// Performs various tasks during subprocess execution.
        /// </summary>
        protected override void OnSubprocessStep()
        {
            var sub = this.Subprocess;
            if (Subprocess.Status == ProcessStatus.Paused)
            {
                this.Pause();
            }
            if (Subprocess.Status == ProcessStatus.Aborted)
            {
                this.Abort();
            }
            if (Subprocess.Status == ProcessStatus.Completed)
            {
                Progress += SubprocessCompletionProgress;
                SetupNextProcess();
            }
            if (this.Status == ProcessStatus.Completed)
            {
                this.CheckLastSubprocess((ILoopDetectionProcess)sub);
            }
            RaisePostStepEvents();
        }

        /// <summary>
        /// Retrieves detected loop from the last process in line.
        /// </summary>
        /// <param name="process">The process to get loop data from.</param>
        private void CheckLastSubprocess(ILoopDetectionProcess process)
        {
            this.LoopStart = process.LoopStart;
            this.LoopLength = process.LoopLength;

            this.EstimatedAccuracy = process.EstimatedAccuracy;
        }

        public CompoundLoopDetectionProcess(IEnumerable<Func<ITrackingExtendedProcess, ITrackingExtendedProcess>> transforms)
            : this("Loop detection process", transforms)
        {
        }
    }
}
