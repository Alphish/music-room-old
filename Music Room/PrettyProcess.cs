using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

using Alphicsh.Interfaces.Processes;

namespace Music_Room_Application
{
    /// <summary>
    /// A wrapper for extended processes, allowing change of progress bar colours.
    /// </summary>
    public class PrettyProcess : ProcessWrapper<ITrackingExtendedProcess>
    {
        /// <summary>
        /// Creates a new pretty wrapper for a given process.
        /// </summary>
        /// <param name="subprocess">The process wrapped.</param>
        public PrettyProcess(ITrackingExtendedProcess subprocess)
            : base(subprocess.Name)
        {
            this.Subprocess = subprocess;
            this.Target = Subprocess.Target;
            this._FrontBrushStatus = ProcessStatus.Idle;
        }

        /// <summary>
        /// Performs tasks after the subprocess finished its action.
        /// </summary>
        protected override void OnSubprocessStep()
        {
            this.Status = Subprocess.Status;
            if (this._FrontBrushStatus != this.Status)
            {
                this._FrontBrushStatus = this.Status;
                this.NotifyPropertyChanged("FrontBrush");
            }
            if (this.Status != ProcessStatus.Completed)
            {
                this.Progress = Subprocess.Progress;
                this.Target = Subprocess.Target;
            }
            else
            {
                this.Progress = (this.Subprocess is NoProcess) ? 0 : 1;
                this.Target = 1;
            }

            RaisePostStepEvents();
        }

        /// <summary>
        /// The foreground progress bar brush.
        /// </summary>
        //and don't even get me started on the green tint applied on the front
        public Brush FrontBrush
        {
            get
            {
                switch (_FrontBrushStatus)
                {
                    case ProcessStatus.Idle:
                        return new SolidColorBrush(Color.FromRgb(0, 0, 0));
                    case ProcessStatus.Active:
                        return new SolidColorBrush(Color.FromRgb(0, 128, 255));
                    case ProcessStatus.Paused:
                        return new SolidColorBrush(Color.FromRgb(192, 192, 0));
                    case ProcessStatus.Aborted:
                        return new SolidColorBrush(Color.FromRgb(192, 0, 0));
                    case ProcessStatus.Completed:
                        return new SolidColorBrush(Color.FromRgb(0, 192, 0));
                    default:
                        return new SolidColorBrush(Color.FromRgb(0, 0, 0));
                }
            }
            set { }
        }
        private ProcessStatus _FrontBrushStatus;

        /// <summary>
        /// The background brush for a progress bar.
        /// </summary>
        public Brush BackBrush
        {
            get
            {
                return new SolidColorBrush(this.Subprocess is NoProcess ? Color.FromRgb(0, 0, 0) : Color.FromRgb(128, 128, 128));
            }
            set { }
        }

        /// <summary>
        /// A NoProcess wrapper.
        /// </summary>
        public static PrettyProcess Empty
        {
            get
            {
                if (_Empty == null)
                {
                    _Empty = new PrettyProcess(new NoProcess());
                }
                return _Empty;
            }
        }
        private static PrettyProcess _Empty;
    }
}
