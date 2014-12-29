using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Music_Room_Application
{
    /// <summary>
    /// Variables used in one way or another across entire application.
    /// </summary>
    public class AppDataContext
    {
        //singleton implementation
        public static AppDataContext Instance
        {
            get
            {
                if (_Instance == null)
                {
                    _Instance = new AppDataContext();
                }
                return _Instance;
            }
        }
        private static AppDataContext _Instance;

        private AppDataContext()
        {
            Player = new MusicRoomPlayer();
        }

        /// <summary>
        /// The tracks player.
        /// </summary>
        public MusicRoomPlayer Player { get; private set; }

    }
}
