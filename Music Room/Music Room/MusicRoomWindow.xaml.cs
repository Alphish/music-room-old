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
using System.Windows.Navigation;
using System.Windows.Shapes;

using Microsoft.Win32;

using Alphicsh.Audio.Metadata.Id3v2;
using Alphicsh.Audio.Metadata.Id3v2.Frames;
using Alphicsh.Audio.Streaming;
using Alphicsh.Interfaces.Processes;
using NAudio.Wave;

namespace Music_Room_Application.Music_Room
{
    /// <summary>
    /// Interaction logic for MusicRoomWindow.xaml
    /// </summary>
    public partial class MusicRoomWindow : Window
    {
        private MusicRoomDataContext Context;

        /// <summary>
        /// Creates the main application window.
        /// </summary>
        public MusicRoomWindow()
        {
            InitializeComponent();

            Context = new MusicRoomDataContext();
            this.DataContext = Context;

            Context.Playlist = new PlaylistManager();

            Context.Playlist.SetupPlaylistGridColumns(
                this.PlaylistGrid,
                new Dictionary<String, String>() {
                    { "Album", "Album" },
                    { "Title", "Title" }
                }
                );

            this.SetupPlaylistGrid();
            this.CurrentPlaylistPath = null;
        }

        #region Playlist section handling

        //event handler for opening loop detection window with tracks without loops detected
        private void UndetectedSearch_Click(object sender, RoutedEventArgs e)
        {
            var loopDetector = new Loop_Detection.LoopDetectionWindow(Context.Playlist.Data.Where(info => info.LoopStart < 0));
            loopDetector.Show();
        }

        //event handler for opening loop detection window with all tracks in the playlist
        private void CompleteSearch_Click(object sender, RoutedEventArgs e)
        {
            var loopDetector = new Loop_Detection.LoopDetectionWindow(Context.Playlist.Data);
            loopDetector.Show();
        }

        //event handler for playing a track
        private void PlaylistEntry_DoubleClick(object sender, RoutedEventArgs e)
        {
            var info = sender as TrackInfo;
            if (info.HasLoop)
            {
                Context.Player.Play(info);
            }
            else
            {
                MessageBox.Show("This track doesn't have loop detected, and thus cannot be played.");
            }
        }

        //handling events for playlist tracks manipulation (adding tracks, making new playlist etc.)

        #region Playlist manipulation

        //the path to save current playlist to
        private String CurrentPlaylistPath;

        //a helper method for various file-dialog related actions
        private void ExecuteFileDialog(Func<FileDialog> dialogCreator, String extensions, String defaultExt, Action<IEnumerable<String>> fileAction)
        {
            var dialog = dialogCreator();
            dialog.Filter = extensions;
            dialog.DefaultExt = defaultExt;

            Nullable<bool> result = dialog.ShowDialog();
            if (result == true)
            {
                fileAction(dialog.FileNames);
            }
        }

        //event handler for adding a track to playlist
        private void AddTrack_Click(object sender, RoutedEventArgs e)
        {
            ExecuteFileDialog(
                () => { var result = new OpenFileDialog(); result.Multiselect = true; return result; },
                "MP3 audio (*.mp3)|*.mp3|Microsoft Wave (*.wav)|*.wav",
                ".mp3",
                (paths) => { Context.Playlist.AddTracks(paths); }
                );
        }

        //event handler for adding existing playlist tracks
        private void AddPlaylist_Click(object sender, RoutedEventArgs e)
        {
            ExecuteFileDialog(
                () => { var result = new OpenFileDialog(); result.Multiselect = true; return result; },
                "Music Room Playlist (*.mrpl)|*.mrpl",
                ".mrpl",
                (paths) => { Context.Playlist.AddPlaylists(paths); }
                );
        }

        //event handler for opening existing playlist
        private void Command_Open(object sender, ExecutedRoutedEventArgs e)
        {
            ExecuteFileDialog(
                () => new OpenFileDialog(),
                "Music Room Playlist (*.mrpl)|*.mrpl",
                ".mrpl",
                (paths) => { Context.Playlist.LoadPlaylist(paths.First()); this.CurrentPlaylistPath = paths.First(); }
                );
        }
        //event handler for making a new playlist
        private void Command_New(object sender, ExecutedRoutedEventArgs e)
        {
            this.CurrentPlaylistPath = null;
            this.Context.Playlist.ClearPlaylist();
        }
        //event handler for saving the current playlist
        private void Command_Save(object sender, ExecutedRoutedEventArgs e)
        {
            if (this.CurrentPlaylistPath == null)
            {
                this.SavePlaylistAs();
            }
            else
            {
                this.Context.Playlist.SavePlaylist(this.CurrentPlaylistPath);
            }
        }
        //event handler for saving the current playlist to a new location
        private void Command_SavePlaylistAs(object sender, RoutedEventArgs e)
        {
            this.SavePlaylistAs();
        }

        //a function to saving a playlist to selected destination
        private void SavePlaylistAs()
        {
            ExecuteFileDialog(
                () => new SaveFileDialog(),
                "Music Room Playlist (*.mrpl)|*.mrpl",
                ".mrpl",
                (paths) =>
                {
                    Context.Playlist.SavePlaylist(paths.First());
                    this.CurrentPlaylistPath = paths.First();
                }
                );
        }

        #endregion

        //TO DO: learn Drag & Drop in WPF properly and actually make it somehow ordered
        //I won't mind if someone decides to help with this one ^^"

        //handling files dragging

        #region Files dragging

        private void Playlist_PreviewDragEnter(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.None;

            if (e.Data.GetDataPresent(DataFormats.FileDrop, true))
            {
                e.Effects = DragDropEffects.Copy;
            }
        }
        private void Playlist_PreviewDrop(object sender, DragEventArgs e)
        {
            var data = (String[])e.Data.GetData(DataFormats.FileDrop, true);
            if (data == null)
            {
                return;
            }
            foreach (var element in data)
            {
                Context.Playlist.AddElement(element);
            }
        }

        #endregion

        //handling dragging items on playlist itself
        //based on that, if I recall: http://www.c-sharpcorner.com/UploadFile/raj1979/drag-and-drop-datagrid-row-in-wpf/

        #region Tracks dragging

        private void SetupPlaylistGrid()
        {
            PlaylistGrid.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(PlaylistDataGrid_PreviewMouseLeftButtonDown);
            PlaylistGrid.Drop += new DragEventHandler(PlaylistDataGrid_Drop);
        }
        private delegate Point GetPosition(IInputElement element);
        public Int32 rowIndex = -1;

        void PlaylistDataGrid_Drop(object sender, DragEventArgs e)
        {
            if (rowIndex < 0)
                return;
            int index = this.GetCurrentRowIndex(e.GetPosition);            
            if (index < 0)
                return;
            if (index == rowIndex)
                return;            
            if (index == PlaylistGrid.Items.Count - 1)
            {
                MessageBox.Show("This row-index cannot be drop");
                return;
            }
            var pl = Context.Playlist.Data;
            TrackInfo info = pl[rowIndex];
            pl.RemoveAt(rowIndex);
            pl.Insert(index, info);
        }
        void PlaylistDataGrid_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            rowIndex = GetCurrentRowIndex(e.GetPosition);
            if (rowIndex < 0)
                return;
            PlaylistGrid.SelectedIndex = rowIndex;
            var info = PlaylistGrid.SelectedItem as TrackInfo;
            if (info == null)
                return;
            if (e.ClickCount == 1)
            {
                DragDropEffects dragdropeffects = DragDropEffects.Move;

                if (DragDrop.DoDragDrop(PlaylistGrid, new DataObject("TrackInfo", info), dragdropeffects)
                                    != DragDropEffects.None)
                {
                    PlaylistGrid.SelectedItem = info;
                }
            }
            if (e.ClickCount == 2)
            {
                this.PlaylistEntry_DoubleClick(info, new RoutedEventArgs());
            }
        }
        private bool GetMouseTargetRow(Visual theTarget, GetPosition position)
        {
            Rect rect = VisualTreeHelper.GetDescendantBounds(theTarget);
            Point point = position((IInputElement)theTarget);
            return rect.Contains(point);            
        }
        private DataGridRow GetRowItem(int index)
        {
            if (PlaylistGrid.ItemContainerGenerator.Status
                    != System.Windows.Controls.Primitives.GeneratorStatus.ContainersGenerated)
                return null;
            return PlaylistGrid.ItemContainerGenerator.ContainerFromIndex(index)
                                                            as DataGridRow;
        }
        private int GetCurrentRowIndex(GetPosition pos)
        {
            int curIndex = -1;
            for (int i = 0; i < PlaylistGrid.Items.Count; i++)
            {
                DataGridRow itm = GetRowItem(i);
                if (itm != null && GetMouseTargetRow(itm, pos))
                {
                    curIndex = i;
                    break;
                }
            }
            return curIndex;
        }

        #endregion

        #endregion

        //event handler for stopping the currently playing track
        private void StopTrack_Click(object sender, RoutedEventArgs e)
        {
            this.Context.Player.Stop();
        }

        //event handler for showing "About" window
        private void ShowAbout(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Music Room, version 0.2.2\nReleased at 28.12.2014\n\nMade using NAudio: http://naudio.codeplex.com");
        }
    }
}
