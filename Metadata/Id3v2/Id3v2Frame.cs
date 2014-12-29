using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Alphicsh.Audio.Metadata.Id3v2.Frames;

namespace Alphicsh.Audio.Metadata.Id3v2
{
    /// <summary>
    /// Base class for ID3v2 tag frame.
    /// </summary>
    public abstract class Id3v2Frame : IId3v2Frame
    {
        /// <summary>
        /// The frame header.
        /// </summary>
        public Id3v2FrameHeader Header { get; private set; }

        //IId3v2Frame implementation

        public IId3v2Tag Tag { get; private set; }

        public String Id { get { return Header.Id; } }
        public Boolean IsExperimental { get { return Header.IsExperimental; } }
        public Int32 Size { get { return Header.Size; } }
        public abstract Int32 Fullsize { get; }

        public IFrameContent Content { get; protected set; }

        /// <summary>
        /// Creates a new ID3v2 tag frame based on its header.
        /// </summary>
        /// <param name="tag">The tag the frame belongs to.</param>
        /// <param name="header">The frame header information.</param>
        protected Id3v2Frame(IId3v2Tag tag, Id3v2FrameHeader header)
        {
            this.Tag = tag;
            this.Header = header;
        }
    }
}
