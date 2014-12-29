using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NAudio.Wave;
using Alphicsh.Audio.Streaming;
using Alphicsh.Interfaces.Processes;

namespace Music_Room_Application
{
    /// <summary>
    /// A class handling looped playback in the application.
    /// </summary>
    public class MusicRoomPlayer : INotifyPropertyChanged
    {
        //INotifyPropertyChanged implementation

        public event PropertyChangedEventHandler PropertyChanged;
        protected void NotifyPropertyChanged(String name)
        {
            if (PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        /// <summary>
        /// The track being currently played.
        /// </summary>
        public TrackInfo Track
        {
            get { return _Track; }
            set
            {
                if (_Track != value)
                {
                    this._Track = value;
                    this.NotifyPropertyChanged("Track");
                }
            }
        }
        private TrackInfo _Track;

        /// <summary>
        /// The process of loading the track into memory for faster, seamless looping.
        /// </summary>
        public PrettyProcess LoadingProcess
        {
            get { return _LoadingProcess; }
            set
            {
                if (_LoadingProcess != value)
                {
                    this._LoadingProcess = value;
                    this.NotifyPropertyChanged("LoadingProcess");
                }
            }
        }
        private PrettyProcess _LoadingProcess;

        //the actual player
        private WaveOut InnerPlayer;
        //the looping stream used
        private LoopStream PlayedStream;

        /// <summary>
        /// Basic constructor.
        /// </summary>
        public MusicRoomPlayer()
        {
            LoadingProcess = PrettyProcess.Empty;
        }

        /// <summary>
        /// Plays the given track, using its looping points.
        /// </summary>
        /// <param name="track">The track to play.</param>
        public void Play(TrackInfo track)
        {
            //stopping the track previously played
            this.Stop();
            this.InnerPlayer = new WaveOut();

            //setting up the track buffering process
            this.Track = track;
            var process = new MemoryWaveStreamBuilder(track.Path);
            process.ProcessCompleted += (sender, e) => {
                //once buffering is completed, the loop stream is created
                var innerStream = (sender as MemoryWaveStreamBuilder).Result;
                this.PlayedStream = new LoopStream(innerStream, track.LoopStart, track.LoopStart + track.LoopLength);
                InnerPlayer.Init(PlayedStream);
                InnerPlayer.Play();
            };
            this.LoadingProcess = new PrettyProcess(process);
            process.RunAsync();
        }

        /// <summary>
        /// Stopping the currently played track.
        /// I guess disposing might be a bit too much; might need to look more into WaveOut later.
        /// </summary>
        public void Stop()
        {
            if (InnerPlayer != null)
            {
                InnerPlayer.Dispose();
            }
        }

        /// <summary>
        /// Moves the currently played track 3 seconds before its looping point.
        /// Mainly for diagnostic purposes, when checking if the loop is seamless or not.
        /// </summary>
        public void SeekBefore()
        {
            if (this.PlayedStream != null)
            {
                this.PlayedStream.Position = PlayedStream.LoopEnd - PlayedStream.WaveFormat.SampleRate * 3;
            }
        }
    }
}
