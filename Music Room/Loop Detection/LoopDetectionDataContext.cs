using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

using Alphicsh.Interfaces.Processes;
using Alphicsh.Audio.Analysis;
using Alphicsh.Audio.Analysis.LoopDetection;

namespace Music_Room_Application.Loop_Detection
{
    /// <summary>
    /// Variables bound in one way or another to Loop Detection window.
    /// </summary>
    public class LoopDetectionDataContext : INotifyPropertyChanged
    {
        //INotifyPropertyChanged implementation

        public event PropertyChangedEventHandler PropertyChanged;
        protected void NotifyPropertyChanged(String name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        /// <summary>
        /// Basic constructor.
        /// </summary>
        public LoopDetectionDataContext()
        {
            this.TrackProcess = PrettyProcess.Empty;
            this.Subprocess = PrettyProcess.Empty;
            this.Iteration = PrettyProcess.Empty;

            this.Playlist = new PlaylistManager();
            this.PendingProcesses = new List<ILoopDetectionProcess>();

            this.InitLoopDetectionMethods();

            this.OutputText = "This textbox will be used later for output...";
        }
        /// <summary>
        /// Constructor filling tracks to find the loops in with provided collection.
        /// </summary>
        /// <param name="infos">The tracks provided.</param>
        public LoopDetectionDataContext(IEnumerable<TrackInfo> infos)
            : this()
        {
            this.Playlist.AddTracks(infos);
        }

        /// <summary>
        /// The main loop detection process in the track.
        /// </summary>
        public PrettyProcess TrackProcess
        {
            get { return _TrackProcess; }
            set
            {
                if (_TrackProcess != value)
                {
                    _TrackProcess = value;
                    this.NotifyPropertyChanged("TrackProcess");
                    this.NotifyPropertyChanged("SubprocessString");
                }
            }
        }
        private PrettyProcess _TrackProcess;

        /// <summary>
        /// The subprocess of the main loop detection process.
        /// </summary>
        public PrettyProcess Subprocess
        {
            get { return _Subprocess; }
            set
            {
                if (_Subprocess != value)
                {
                    _Subprocess = value;
                    this.NotifyPropertyChanged("Subprocess");
                    this.NotifyPropertyChanged("SubprocessString");
                }
            }
        }
        private PrettyProcess _Subprocess;

        /// <summary>
        /// The iteration of the loop detection subprocess; typically of the loop detection algorithm itself.
        /// </summary>
        public PrettyProcess Iteration
        {
            get { return _Iteration; }
            set
            {
                if (_Iteration != value)
                {
                    _Iteration = value;
                    this.NotifyPropertyChanged("Iteration");
                    this.NotifyPropertyChanged("SubprocessString");
                }
            }
        }
        private PrettyProcess _Iteration;

        /// <summary>
        /// The string describing the current loop detection subprocess state.
        /// </summary>
        public String SubprocessString
        {
            get
            {
                var tpp = TrackProcess.Subprocess as ProcessPipeline<ITrackingExtendedProcess>;
                if (TrackProcess.IsFinished || tpp == null)
                {
                    return "Step 0 of 0" + Environment.NewLine + "Processing idleness...";
                }

                return "Step " + (tpp.CurrentIndex + 1) + " of " + tpp.Target + Environment.NewLine
                    + Subprocess.Name + (!(Iteration.Subprocess is NoProcess) ? ": " + Iteration.Name : "");
            }
        }

        /// <summary>
        /// The tracks to detect the loops in.
        /// </summary>
        public PlaylistManager Playlist
        {
            get
            {
                return _Playlist;
            }
            set
            {
                if (_Playlist != value)
                {
                    _Playlist = value;
                    this.NotifyPropertyChanged("Playlist");
                }
            }
        }
        private PlaylistManager _Playlist;

        /// <summary>
        /// The available loop detection methods.
        /// </summary>
        public ObservableCollection<LoopDetectionSpecificMethod> LoopDetectionMethods
        {
            get { return _LoopDetectionMethods; }
            set
            {
                if (_LoopDetectionMethods != value)
                {
                    this._LoopDetectionMethods = value;
                    this.NotifyPropertyChanged("LoopDetectionMethods");
                }
            }
        }
        private ObservableCollection<LoopDetectionSpecificMethod> _LoopDetectionMethods;

        /// <summary>
        /// Currently selected loop detection method.
        /// </summary>
        public LoopDetectionSpecificMethod SelectedMethod
        {
            get { return _SelectedMethod; }
            set
            {
                if (_SelectedMethod != value)
                {
                    this._SelectedMethod = value;
                    this.NotifyPropertyChanged("SelectedMethod");
                }
            }
        }
        private LoopDetectionSpecificMethod _SelectedMethod;

        /// <summary>
        /// Loop detection methods setup.
        /// </summary>
        private void InitLoopDetectionMethods()
        {
            //TO DO: make methods readable from configuration file
            this.LoopDetectionMethods = new ObservableCollection<LoopDetectionSpecificMethod>();

            //two loops (front) family
            this.LoopDetectionMethods.Add(new LoopDetectionSpecificMethod("Two loops",
                SysaldisLoopDetection.BasicSysaldis, new Dictionary<String, Object>()
                {
                    { "refStart", 0.2 },
                    { "refEnd", 0.3 },
                    { "offMin", 0.33 },
                    { "offMax", 0.5 },
                    { "matches", "2x49 3x147 4x441 5x1323" }
                }));
            this.LoopDetectionMethods.Add(new LoopDetectionSpecificMethod("Two loops (trim)",
                SysaldisLoopDetection.TrimmedSysaldis, new Dictionary<String, Object>()
                {
                    { "refStart", 0.2 },
                    { "refEnd", 0.3 },
                    { "offMin", 0.33 },
                    { "offMax", 0.5 },
                    { "matches", "2x49 3x147 4x441 5x1323" }
                }));
            this.LoopDetectionMethods.Add(new LoopDetectionSpecificMethod("Two loops (long)",
                SysaldisLoopDetection.BasicSysaldis, new Dictionary<String, Object>()
                {
                    { "refStart", 0.2 },
                    { "refEnd", 0.3 },
                    { "offMin", 0.33 },
                    { "offMax", 0.5 },
                    { "matches", "3x147 4x441 5x1323" }
                }));

            //two loops (backtrack) family
            this.LoopDetectionMethods.Add(new LoopDetectionSpecificMethod("Echoed backtrack",
                SysaldisLoopDetection.EchoedBacktrack, new Dictionary<String, Object>()
                {
                    { "matches", "2x49 3x147 4x441 5x1323" }
                }));
            this.LoopDetectionMethods.Add(new LoopDetectionSpecificMethod("Echoed backtrack (long)",
                SysaldisLoopDetection.EchoedBacktrack, new Dictionary<String, Object>()
                {
                    { "matches", "3x147 4x441 5x1323" }
                }));

            //one loop (front) family
            this.LoopDetectionMethods.Add(new LoopDetectionSpecificMethod("One loop",
                SysaldisLoopDetection.OneLoop, new Dictionary<String, Object>()
                {
                    { "matches", "2x49 3x147 4x441 5x1323" }
                }));
            this.LoopDetectionMethods.Add(new LoopDetectionSpecificMethod("One loop (long)",
                SysaldisLoopDetection.OneLoop, new Dictionary<String, Object>()
                {
                    { "matches", "3x147 4x441 5x1323" }
                }));

            //one loop (backtrack) family
            this.LoopDetectionMethods.Add(new LoopDetectionSpecificMethod("Backtrack",
                SysaldisLoopDetection.Backtrack, new Dictionary<String, Object>()
                {
                    { "matches", "2x49 3x147 4x441 5x1323" }
                }));
            this.LoopDetectionMethods.Add(new LoopDetectionSpecificMethod("Backtrack (long)",
                SysaldisLoopDetection.Backtrack, new Dictionary<String, Object>()
                {
                    { "matches", "3x147 4x441 5x1323" }
                }));

            this.SelectedMethod = LoopDetectionMethods.First();
        }

        /// <summary>
        /// The loop detection processes to be executed later.
        /// </summary>
        public IList<ILoopDetectionProcess> PendingProcesses
        {
            get { return _PendingProcesses; }
            set
            {
                if (_PendingProcesses != value)
                {
                    _PendingProcesses = value;
                    this.NotifyPropertyChanged("PendingProcesses");
                }
            }
        }
        private IList<ILoopDetectionProcess> _PendingProcesses;

        /// <summary>
        /// Pending processes count for binding purpose.
        /// TO DO: change it to something more elegant (especially since it's the program that keeps count in sync with PendingProcesses at the moment).
        /// </summary>
        public Int32 PendingProcessesCount
        {
            get { return _PendingProcessesCount; }
            set
            {
                if (_PendingProcessesCount != value)
                {
                    _PendingProcessesCount = value;
                    this.NotifyPropertyChanged("PendingProcessesCount");
                }
            }
        }
        private Int32 _PendingProcessesCount;

        /// <summary>
        /// The track being currently processed.
        /// </summary>
        public TrackInfo CurrentTrack
        {
            get { return _CurrentTrack; }
            set
            {
                if (_CurrentTrack != value)
                {
                    _CurrentTrack = value;
                    this.NotifyPropertyChanged("CurrentTrack");
                }
            }
        }
        private TrackInfo _CurrentTrack;

        /// <summary>
        /// Creating and running loop detection processes for selected tracks.
        /// A preparation action can be provided for setting up each newly created process (e.g. event subscription).
        /// Not quite sure if data context class is the most appropriate place for that...
        /// </summary>
        /// <param name="tracks">The tracks to perform loop detection on.</param>
        /// <param name="preparation">The preparation action to be performed on each newly created process.</param>
        public void RunAlgorithm(IEnumerable<TrackInfo> tracks, Action<ILoopDetectionProcess> preparation)
        {
            //clearing all ongoing processes beforehand
            this.AbortAll();

            //building loop detection processes
            foreach (var info in tracks)
            {
                var process = SelectedMethod.Search(info);
                preparation(process);
                process.ProcessFinished += (sender, e) => this.RunNext();
                PendingProcesses.Add(process);
            }
            PendingProcessesCount = PendingProcesses.Count;

            //running the first process of selected
            this.RunNext();
        }

        /// <summary>
        /// Aborts the ongoing loop detection process.
        /// </summary>
        public void AbortCurrent()
        {
            if (!TrackProcess.IsFinished)
            {
                this.TrackProcess.Abort();
            }
        }
        /// <summary>
        /// Aborts the ongoing loop detection process and proceeds to the next one, if any.
        /// </summary>
        public void SkipCurrent()
        {
            this.AbortCurrent();
            this.RunNext();
        }
        /// <summary>
        /// Aborts all loop detection processes.
        /// </summary>
        public void AbortAll()
        {
            PendingProcesses.Clear();
            PendingProcessesCount = 0;
            this.AbortCurrent();
        }

        /// <summary>
        /// Runs the next process in line.
        /// </summary>
        private void RunNext()
        {
            if (!TrackProcess.IsFinished)
            {
                TrackProcess.Abort();
            }

            if (!PendingProcesses.Any())
            {
                return;
            }

            this.TrackProcess = new PrettyProcess(PendingProcesses.First() as ITrackingExtendedProcess);
            PendingProcesses.RemoveAt(0);
            this.PendingProcessesCount--;
            this.TrackProcess.RunAsync();
        }

        /// <summary>
        /// The loop detection processes output.
        /// Generally, it's planned to have some funny numbers printed to, which might or might not indicate how successful the process was.
        /// </summary>
        public String OutputText
        {
            get { return _OutputText; }
            set
            {
                if (_OutputText != value)
                {
                    this._OutputText = value;
                    this.NotifyPropertyChanged("OutputText");
                }
            }
        }
        private String _OutputText;

        /// <summary>
        /// The track player.
        /// </summary>
        public MusicRoomPlayer Player
        {
            get { return AppDataContext.Instance.Player; }
        }
    }
}

