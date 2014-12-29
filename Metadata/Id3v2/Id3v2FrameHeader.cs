using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alphicsh.Audio.Metadata.Id3v2
{
    /// <summary>
    /// A structure representing ID3v2 tag frame data.
    /// </summary>
    public struct Id3v2FrameHeader
    {
        /// <summary>
        /// Gets the frame identifier.
        /// </summary>
        public String Id { get { return _Id; } }
        private String _Id;
        /// <summary>
        /// Indicates whether the frame header is valid or not.
        /// </summary>
        public Boolean IsValid { get { return (this.Id != null); } }
        /// <summary>
        /// Indicates whether the frame is experimental or not.
        /// </summary>
        public Boolean IsExperimental { get { return (this.Id[0] >= 'X' && this.Id[0] <= 'Z'); } }

        /// <summary>
        /// Gets the frame size.
        /// </summary>
        public Int32 Size { get { return _Size; } }
        private Int32 _Size;
        /// <summary>
        /// Gets the frame status flags.
        /// </summary>
        public Byte StatusFlags { get { return _StatusFlags; } }
        private Byte _StatusFlags;
        /// <summary>
        /// Gets the frame format flags.
        /// </summary>
        public Byte FormatFlags { get { return _FormatFlags; } }
        private Byte _FormatFlags;

        /// <summary>
        /// Creates an ID3v2 tag frame header from bytes provided.
        /// </summary>
        /// <param name="bytes">The bytes to read the frame header from.</param>
        public Id3v2FrameHeader(Byte[] bytes)
        {
            _Id = null;
            _Size = -1;
            _StatusFlags = 0;
            _FormatFlags = 0;

            if (BytesAreValid(bytes))
            {
                _Id = System.Text.Encoding.ASCII.GetString(bytes, 0, 4);
                _Size = 0;
                for (var i = 4; i < 8; i++)
                {
                    _Size *= 0x100;
                    _Size += bytes[i];
                }
                _StatusFlags = bytes[8];
                _FormatFlags = bytes[9];
            }
        }
        //checks if bytes provided correspond to an actual tag
        private Boolean BytesAreValid(Byte[] bytes)
        {
            //checking bytes length
            if (bytes.Length != 10)
            {
                return false;
            }

            //checking if identifier matches 0-9A-Z
            for (var i = 0; i < 4; i++)
            {
                var idChar = (Char)bytes[i];
                if (!(idChar >= '0' && idChar <= '9') && !(idChar >= 'A' && idChar <= 'Z'))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
