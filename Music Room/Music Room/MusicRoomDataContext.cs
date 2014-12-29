using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

using Alphicsh.Interfaces.Processes;

namespace Music_Room_Application.Music_Room
{
    /// <summary>
    /// Variables bound in one way or another to Music Room window.
    /// </summary>
    public class MusicRoomDataContext : INotifyPropertyChanged
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
        /// The source of playlist data.
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
        /// The track player.
        /// </summary>
        public MusicRoomPlayer Player
        {
            get { return AppDataContext.Instance.Player; }
        }
    }
}
