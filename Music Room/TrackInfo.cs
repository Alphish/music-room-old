using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

using Alphicsh.Audio.Metadata.Id3v2;
using Alphicsh.Audio.Metadata.Id3v2.Frames;

namespace Music_Room_Application
{
    /// <summary>
    /// A class for basic soundtrack information.
    /// </summary>
    public class TrackInfo : INotifyPropertyChanged
    {
        //INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String name)
        {
            if (PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        /// <summary>
        /// Creates a new soundtrack information from an audio file.
        /// </summary>
        /// <param name="path">The path of the file.</param>
        public TrackInfo(String path)
        {
            this.Path = path;

            Title = System.IO.Path.GetFileNameWithoutExtension(path);
            Album = "";

            //trying to read track information from ID3v2 tag, if available
            var tag = Id3v2Tag.Create(path);
            IId3v2Frame titleFrame = null;
            IId3v2Frame albumFrame = null;
            if (tag != null)
            {
                titleFrame = tag.Frame("TIT2");
                albumFrame = tag.Frame("TALB");
            }
            if (titleFrame != null)
            {
                this.Title = (titleFrame.Content as ITextFrameContent).Text;
            }
            if (albumFrame != null)
            {
                this.Album = (albumFrame.Content as ITextFrameContent).Text;
            }

            //setting loop data to empty
            LoopStart = -1;
            LoopLength = -1;
            LoopState = "-";
        }

        /// <summary>
        /// Creates a new soundtrack information from XML element.
        /// Might drop XML parsing in favour of something more general at some point.
        /// </summary>
        /// <param name="source">The XML node to read data from.</param>
        /// <returns>A soundtrack information instance.</returns>
        public static TrackInfo FromXElement(XElement source)
        {
            if (source.Name.LocalName != "track")
            {
                throw new FormatException("The track should be stored in a \"track\" element.");
            }

            //basic reading of track information
            var path = source.Value;
            var result = new TrackInfo(path);

            //reading loop data, if provided
            if (source.Attribute("loopStart") != null)
            {
                var start = source.Attribute("loopStart").Value;
                var length = source.Attribute("loopLength").Value;

                result.LoopStart = Convert.ToInt32(start);
                result.LoopLength = Convert.ToInt32(length);
                result.LoopState = result.LoopLength + " from " + result.LoopStart;
            }

            return result;
        }

        /// <summary>
        /// Converts a soundtrack information to XML element.
        /// Might drop XML parsing in favour of something more general at some point.
        /// </summary>
        /// <returns>XML element containing the soundtrack information.</returns>
        public XElement ToXElement()
        {
            var result = new XElement("track");
            if (HasLoop)
            {
                result.SetAttributeValue("loopStart", this.LoopStart);
                result.SetAttributeValue("loopLength", this.LoopLength);
            }
            result.Value = this.Path;
            return result;
        }

        /// <summary>
        /// The path of the audio file.
        /// </summary>
        public String Path
        {
            get { return _Path; }
            set
            {
                if (_Path != value)
                {
                    _Path = value;
                    this.NotifyPropertyChanged("Path");
                }
            }
        }
        private String _Path;

        /// <summary>
        /// The track title.
        /// </summary>
        public String Title
        {
            get { return _Title; }
            set
            {
                if (_Title != value)
                {
                    this._Title = value;
                    this.NotifyPropertyChanged("Title");
                }
            }
        }
        private String _Title;

        /// <summary>
        /// The track album.
        /// </summary>
        public String Album
        {
            get { return _Album; }
            set
            {
                if (_Album != value)
                {
                    this._Album = value;
                    this.NotifyPropertyChanged("Album");
                }
            }
        }
        private String _Album;

        /// <summary>
        /// The loop state string for display.
        /// Will probably get dropped at some point.
        /// </summary>
        public String LoopState
        {
            get { return _LoopState; }
            set
            {
                if (_LoopState != value)
                {
                    this._LoopState = value;
                    this.NotifyPropertyChanged("LoopState");
                }
            }
        }
        private String _LoopState;

        /// <summary>
        /// The loop starting sample index.
        /// </summary>
        public Int32 LoopStart
        {
            get { return _LoopStart; }
            set
            {
                if (_LoopStart != value)
                {
                    this._LoopStart = value;
                    this.NotifyPropertyChanged("LoopStart");
                    this.NotifyPropertyChanged("HasLoop");
                }
            }
        }
        private Int32 _LoopStart;

        /// <summary>
        /// The loop length (in samples).
        /// </summary>
        public Int32 LoopLength
        {
            get { return _LoopLength; }
            set
            {
                if (_LoopLength != value)
                {
                    this._LoopLength = value;
                    this.NotifyPropertyChanged("LoopLength");
                }
            }
        }
        private Int32 _LoopLength;

        /// <summary>
        /// Tells whether a track has a loop detected or not.
        /// </summary>
        public Boolean HasLoop
        {
            get { return _LoopStart > -1; }
        }
    }
}
