using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Alphicsh.Audio.Metadata.Id3v2.Frames;

namespace Alphicsh.Audio.Metadata.Id3v2.V23
{
    /// <summary>
    /// A class for ID3v2.3 frame.
    /// </summary>
    public class Id3v23Frame : Id3v2Frame
    {
        public override int Fullsize
        {
            get { return this.Size + 10; }
        }

        /// <summary>
        /// Creates a new ID3v2.3 frame for a given tag, header and contained data.
        /// </summary>
        /// <param name="tag">The tag the frame belongs to.</param>
        /// <param name="header">The frame header information.</param>
        /// <param name="data">The frame data.</param>
        public Id3v23Frame(IId3v2Tag tag, Id3v2FrameHeader header, Byte[] data)
            : base(tag, header)
        {
            Content = CreateContent(this.Id, data);
        }

        //creates frame contents based on frame identifier
        private IFrameContent CreateContent(String id, Byte[] data)
        {
            Func<String> decodeText = () => FrameTextDecoder.Instance.Decode(this.Tag.Version, this.Tag.Revision, false, data);
            Func<String> decodeFullText = () => FrameTextDecoder.Instance.Decode(this.Tag.Version, this.Tag.Revision, true, data);

            switch (id)
            {
                //textual frames
                case "TALB": case "TCON": case "TCOP": case "TDAT": case "TENC":
                case "TIT1": case "TIT2": case "TIT3":
                case "TKEY": case "TMED": case "TOAL": case "TOAF":
                case "TOWN": case "TPE3": case "TPOS": case "TPUB":
                case "TRCK": case "TRDA": case "TRSN": case "TRSO":
                case "TSRC": case "TSSE":
                    return new TextFrameContent(decodeText());
                
                //integer frames
                case "TBMP": case "TDLY": case "TLEN":
                case "TORY": case "TYER": case "TSIZ":
                    return new IntegerFrameContent(decodeText());

                //text collection frames
                case "TCOM": case "TEXT": case "TOLY": case "TPE1": case "TPE2": case "TPE4":
                    return new TextCollectionFrameContent(decodeText());

                //unsupported frame type
                default:
                    return null;
            }
        }
    }
}
