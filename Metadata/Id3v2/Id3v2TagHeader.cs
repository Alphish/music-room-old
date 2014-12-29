using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alphicsh.Audio.Metadata.Id3v2
{
    /// <summary>
    /// A structure representing ID3v2 tag header data.
    /// </summary>
    public struct Id3v2TagHeader
    {
        /// <summary>
        /// Gets the ID3v2 version of the tag.
        /// </summary>
        public Byte Version { get { return _Version; } }
        private Byte _Version;

        /// <summary>
        /// Gets the revision of the ID3v2 tag version.
        /// </summary>
        public Byte Revision { get { return _Revision; } }
        private Byte _Revision;

        /// <summary>
        /// Indicates whether the tag header is valid or not.
        /// </summary>
        public Boolean IsValid { get { return (Version < 0xFF); } }

        /// <summary>
        /// Gets the tag flags.
        /// </summary>
        public Byte Flags { get { return _Flags; } }
        private Byte _Flags;

        /// <summary>
        /// Gets the tag size.
        /// </summary>
        public Int32 Size { get { return _Size; } }
        private Int32 _Size;

        /// <summary>
        /// Creates a new ID3v2 tag header based on provided 10 bytes.
        /// </summary>
        /// <param name="bytes">The header bytes.</param>
        public Id3v2TagHeader(Byte[] bytes)
        {
            _Version = 0xFF;
            _Revision = 0xFF;
            _Flags = 0;
            _Size = -1;

            if (BytesAreValid(bytes))
            {
                _Version = bytes[3];
                _Revision = bytes[4];
                _Flags = bytes[5];

                _Size = 0;
                for (var i = 6; i < 10; i++)
                {
                    _Size *= 0x80;
                    _Size += bytes[i];
                }
            }
        }
        //checks if the bytes given represent an actual header
        private Boolean BytesAreValid(Byte[] bytes)
        {
            //invalid length
            if (bytes.Length != 10)
            {
                return false;
            }

            //not starting with "ID3"
            if (bytes[0] != 0x49 || bytes[1] != 0x44 || bytes[2] != 0x33)
            {
                return false;
            }
            //invalid version or revision byte
            if (bytes[3] == 0xFF || bytes[4] == 0xFF)
            {
                return false;
            }
            //size byte larger than 127
            for (var i=6; i<10; i++)
            {
                if (bytes[i] >= 0x80)
                {
                    return false;
                }
            }

            //everything all right
            return true;
        }

        /// <summary>
        /// Retrieves a flag at a given position.
        /// </summary>
        /// <param name="index">The flag index, from 0 (least significant bit) to 7 (most significant bit).</param>
        /// <returns>The flag value.</returns>
        public Boolean GetFlag(Byte index)
        {
            if (index >= 8)
            {
                throw new ArgumentException("Cannot get flag of index larger than 7.");
            }
            return (Flags & (1 << index)) > 0;
        }
    }
}
