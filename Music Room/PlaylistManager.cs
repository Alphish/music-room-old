using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
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
using System.Xml.Linq;

namespace Music_Room_Application
{
    /// <summary>
    /// A class for manipulating playlist data.
    /// </summary>
    public class PlaylistManager
    {
        /// <summary>
        /// The tracks stored.
        /// </summary>
        public ObservableCollection<TrackInfo> Data { get; private set; }

        /// <summary>
        /// Basic constructor.
        /// </summary>
        public PlaylistManager()
        {
            Data = new ObservableCollection<TrackInfo>();
        }

        /// <summary>
        /// Creates DataGrid columns and binds them to specific properties.
        /// I've got a feeling it could be handled more elegantly and generically...
        /// </summary>
        /// <param name="playlistGrid">The DataGrid to setup.</param>
        /// <param name="columns">The columns provided as property-header pairs.</param>
        public void SetupPlaylistGridColumns(DataGrid playlistGrid, IDictionary<String, String> columns)
        {
            playlistGrid.AutoGenerateColumns = false;

            playlistGrid.Columns.Clear();
            foreach (var data in columns)
            {
                playlistGrid.Columns.Add(CreateColumn(data));
            }
        }
        private DataGridColumn CreateColumn(KeyValuePair<String, String> data)
        {
            var result = new DataGridTextColumn();
            result.Header = data.Value;
            result.Binding = new Binding(data.Key);
            result.Width = new DataGridLength(1, DataGridLengthUnitType.Star);
            return result;
        }

        /// <summary>
        /// Adds an individual track.
        /// </summary>
        /// <param name="path">The path of the audio file.</param>
        public void AddTrack(String path)
        {
            //skipping not supported formats
            if (!path.EndsWith(".mp3") && !path.EndsWith(".wav") && !path.EndsWith(".ogg")) return;

            this.AddTrack(new TrackInfo(path));
        }
        /// <summary>
        /// Adds an individual track.
        /// </summary>
        /// <param name="info">The audio file information.</param>
        public void AddTrack(FileInfo info)
        {
            AddTrack(info.FullName);
        }
        /// <summary>
        /// Adds an individual track.
        /// </summary>
        /// <param name="info">A track information.</param>
        public void AddTrack(TrackInfo info)
        {
            Data.Add(info);
        }

        /// <summary>
        /// Adds a collection of tracks.
        /// </summary>
        /// <param name="infos">Multiple tracks information.</param>
        public void AddTracks(IEnumerable<TrackInfo> infos)
        {
            foreach (var info in infos)
            {
                Data.Add(info);
            }
        }
        /// <summary>
        /// Adds a collection of tracks.
        /// </summary>
        /// <param name="paths">Multiple audio files paths.</param>
        public void AddTracks(IEnumerable<String> paths)
        {
            foreach (var path in paths)
            {
                this.AddTrack(path);
            }
        }

        /// <summary>
        /// Remove a track from playlist.
        /// </summary>
        /// <param name="track">The track to remove.</param>
        public void RemoveTrack(TrackInfo track)
        {
            this.Data.Remove(track);
        }

        /// <summary>
        /// Removes given tracks from playlist.
        /// </summary>
        /// <param name="tracks">The tracks to remove.</param>
        public void RemoveTracks(IEnumerable<TrackInfo> tracks)
        {
            foreach (var track in tracks)
            {
                this.RemoveTrack(track);
            }
        }

        /// <summary>
        /// Adds a directory with soundtracks.
        /// </summary>
        /// <param name="path">The path of the directory.</param>
        public void AddDirectory(String path)
        {
            this.AddDirectory(new DirectoryInfo(path));
        }
        /// <summary>
        /// Adds a directory with soundtracks.
        /// </summary>
        /// <param name="info">The directory information.</param>
        public void AddDirectory(DirectoryInfo info)
        {
            foreach (var dirInfo in info.GetDirectories())
            {
                this.AddDirectory(dirInfo);
            }
            foreach (var fileInfo in info.GetFiles())
            {
                this.AddTrack(fileInfo.FullName);
            }
        }

        /// <summary>
        /// Adds all tracks from existing playlist.
        /// </summary>
        /// <param name="path">The path of the playlist.</param>
        public void AddPlaylist(String path)
        {
            var playlist = XElement.Load(path);
            if (playlist.Name.LocalName != "playlist")
                throw new FormatException("The playlist should be stored in \"playlist\" element.");

            foreach (var element in playlist.Elements())
            {
                this.AddTrack(TrackInfo.FromXElement(element));
            }
        }
        /// <summary>
        /// Adds all tracks from existing playlists.
        /// </summary>
        /// <param name="paths">The paths of the playlists.</param>
        public void AddPlaylists(IEnumerable<String> paths)
        {
            foreach (var path in paths)
            {
                this.AddPlaylist(path);
            }
        }

        /// <summary>
        /// Adding an element; whether it's an audio file, a directory or a playlist.
        /// </summary>
        /// <param name="path">The path of the element.</param>
        public void AddElement(String path)
        {
            if (File.Exists(path))
            {
                if (path.EndsWith(".mrpl"))
                {
                    this.AddPlaylist(path);
                }
                else
                {
                    this.AddTrack(path);
                }
                return;
            }
            if (Directory.Exists(path))
            {
                this.AddDirectory(path);
                return;
            }

            throw new FileNotFoundException("Couldn't find a file or directory with a given path.", path);
        }

        //I've got a feeling the following cases would better be handled by creating new PlaylistManager instance altogether...

        /// <summary>
        /// Clears a playlist from all its tracks.
        /// </summary>
        public void ClearPlaylist()
        {
            this.Data.Clear();
        }
        /// <summary>
        /// Replaces current data with other playlist.
        /// </summary>
        /// <param name="path">The path of the playlist</param>
        public void LoadPlaylist(String path)
        {
            ClearPlaylist();
            AddPlaylist(path);
        }
        /// <summary>
        /// Saves the playlist to a given location.
        /// </summary>
        /// <param name="path">The path of the playlist.</param>
        public void SavePlaylist(String path)
        {
            var playlist = new XElement("playlist");
            foreach (var track in Data)
            {
                playlist.Add(track.ToXElement());
            }
            playlist.Save(path);
        }
    }
}
