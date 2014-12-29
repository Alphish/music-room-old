using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using Alphicsh.Audio.Analysis;
using Alphicsh.Audio.Analysis.LoopDetection;
using Alphicsh.Audio.Analysis.WaveMatching;
using Alphicsh.Interfaces.Processes;
using Alphicsh.Audio.Streaming;
using NAudio.Wave;

namespace Music_Room_Application.Loop_Detection
{
    /// <summary>
    /// Interaction logic for LoopDetectionWindow.xaml
    /// </summary>
    public partial class LoopDetectionWindow : Window
    {
        private LoopDetectionDataContext Context;

        /// <summary>
        /// Creates a loop detection window with data context provided.
        /// </summary>
        /// <param name="context">The pre-made data context.</param>
        public LoopDetectionWindow(LoopDetectionDataContext context)
        {
            InitializeComponent();

            this.Context = context;
            this.DataContext = Context;

            Context.Subprocess = new PrettyProcess(new NoProcess());
            Context.Iteration = new PrettyProcess(new NoProcess());

            Context.Playlist.SetupPlaylistGridColumns(
                this.PlaylistGrid,
                new Dictionary<String, String>() {
                    { "Album", "Album" },
                    { "Title", "Title" },
                    { "LoopState", "Loop detected" }
                }
                );
        }
        /// <summary>
        /// Creates a loop detection window with no additional data.
        /// </summary>
        public LoopDetectionWindow()
            : this(new LoopDetectionDataContext())
        {
        }
        /// <summary>
        /// Cretes a loop detection window with tracks to detect loops in.
        /// </summary>
        /// <param name="infos"></param>
        public LoopDetectionWindow(IEnumerable<TrackInfo> infos)
            : this(new LoopDetectionDataContext(infos))
        {
        }

        //method for data context subprocess update
        private void UpdateSubprocess(ITrackingExtendedProcess process)
        {
            Context.Subprocess = (process != null) ? new PrettyProcess(process) : PrettyProcess.Empty;
        }
        //method for data context subprocess iteration update
        private void UpdateIteration(ITrackingExtendedProcess process)
        {
            Context.Iteration = (process != null) ? new PrettyProcess(process) : PrettyProcess.Empty;
        }

        //event handler for aborting all ongoing processes
        private void AbortProcess_Click(object sender, RoutedEventArgs e)
        {
            Context.AbortAll();
        }

        //event handler for creating and running loop detection processes
        private void RunButton_Click(object sender, RoutedEventArgs e)
        {
            IEnumerable<TrackInfo> infos;
            //detect for selected items, if any
            if (PlaylistGrid.SelectedItems.Count > 0)
            {
                infos = PlaylistGrid.SelectedItems.OfType<TrackInfo>();
            }
            //gather all tracks in list otherwise
            else
            {
                infos = Context.Playlist.Data;
            }

            //runs all the processes
            Context.RunAlgorithm(infos, (process) => (process as CompoundLoopDetectionProcess).SubprocessPrepared += this.TrackProcess_SubprocessPrepared);
        }
        //event handler for subprocess change; very Sysaldis specific, I guess I'll need to do something with it, eventually...
        private void TrackProcess_SubprocessPrepared(object sender, SubprocessEventArgs<ITrackingExtendedProcess> e)
        {
            UpdateSubprocess(e.Subprocess);
            if (e.Subprocess is SysaldisProcess)
            {
                (e.Subprocess as SysaldisProcess).SubprocessPrepared +=
                    (iterSender, iterArgs) => UpdateIteration(iterArgs.Subprocess);
            }
        }

        //event handler for double-clicking a track
        private void PlaylistEntry_DoubleClick(object sender, RoutedEventArgs e)
        {
            var info = (sender as DataGridRow).Item as TrackInfo;

            if (info.HasLoop)
            {
                Context.Player.Play(info);
            }
            else
            {
                MessageBox.Show("This track doesn't have loop detected, and thus cannot be played.");
            }
        }

        //event handler for stopping a track
        private void StopTrack_Click(object sender, RoutedEventArgs e)
        {
            Context.Player.Stop();
        }

        //event handler for moving a track 3 seconds before looping point, for diagnostic purpose
        private void SeekTrack_Click(object sender, RoutedEventArgs e)
        {
            Context.Player.SeekBefore();
        }
    }
}
