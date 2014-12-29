using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alphicsh.Audio.Metadata.Id3v2.V23
{
    /// <summary>
    /// A class for ID3v2.3 tag.
    /// </summary>
    public class Id3v23Tag : Id3v2Tag
    {
        public override int Fullsize
        {
            get { return this.Size + 10; }
        }

        /// <summary>
        /// Indicates whether the tag is unsynchronized or not.
        /// </summary>
        public Boolean UseUnsynchronisation { get { return Header.GetFlag(7); } }
        /// <summary>
        /// Indicated whether the tag has extended header or not.
        /// </summary>
        public Boolean HasExtendedHeader { get { return Header.GetFlag(6); } }
        /// <summary>
        /// Indicates whether the tag is experimental or not.
        /// </summary>
        public Boolean IsExperimental { get { return Header.GetFlag(5); } }

        /// <summary>
        /// Creates a new ID3v2.3 tag, based on a given header and content reader.
        /// </summary>
        /// <param name="header">The tag header.</param>
        /// <param name="reader">The tag contents reader.</param>
        public Id3v23Tag(Id3v2TagHeader header, BinaryReader reader)
            : base(header)
        {
            if (UseUnsynchronisation)
            {
                throw new NotSupportedException("The unsynchronisation is not supported.");
            }
            if (HasExtendedHeader)
            {
                throw new NotSupportedException("The extended header is not supported.");
            }

            this.ReadFrames(reader);
        }

        //reads the frames as needed, along with padding
        private void ReadFrames(BinaryReader reader)
        {
            Int32 remainingSize = this.Size;
            while (remainingSize > 0)
            {
                remainingSize -= this.ReadFrame(reader);

                if (reader.PeekChar() == 0)
                {
                    break;
                }
            }
        }

        //reads a single frame
        private Int32 ReadFrame(BinaryReader reader)
        {
            var frameHead = new Id3v2FrameHeader(reader.ReadBytes(10));
            var frame = new Id3v23Frame(this, frameHead, reader.ReadBytes(frameHead.Size));
            Frames.Add(frame);
            return frame.Fullsize;
        }
    }
}
