using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Alphicsh.Audio.Metadata.Id3v2.V23;
using Alphicsh.Audio.Metadata.Id3v2.V24;

namespace Alphicsh.Audio.Metadata.Id3v2
{
    /// <summary>
    /// Base class for ID3v2 tag information.
    /// </summary>
    public abstract class Id3v2Tag : IId3v2Tag
    {
        /// <summary>
        /// The tag's header information.
        /// </summary>
        public Id3v2TagHeader Header { get; protected set; }

        //IId3v2Tag implementation

        public Byte Version { get { return Header.Version; } }
        public Byte Revision { get { return Header.Revision; } }
        public Int32 Size { get { return Header.Size; } }
        public abstract Int32 Fullsize { get; }

        public List<IId3v2Frame> Frames { get; private set; }

        public IId3v2Frame Frame(String id)
        {
            return Frames.FirstOrDefault(frame => frame.Id == id);
        }

        /// <summary>
        /// Creates a new ID3v2 tag based on its header. Further information should be provided by derived classes.
        /// </summary>
        /// <param name="header">The tag header.</param>
        protected Id3v2Tag(Id3v2TagHeader header)
        {
            this.Header = header;

            this.Frames = new List<IId3v2Frame>();
        }

        /// <summary>
        /// Retrieves an ID3v2 tag from a file.
        /// </summary>
        /// <param name="filename">The file to read the tag from.</param>
        /// <returns>An ID3v2 tag, or null if none is found.</returns>
        public static Id3v2Tag Create(String filename)
        {
            using (var reader = new BinaryReader(File.Open(filename, FileMode.Open)))
            {
                Id3v2TagHeader header = new Id3v2TagHeader(reader.ReadBytes(10));

                //depending on tag version, an implementation is chosen
                switch (header.Version)
                {
                    case 3:
                        return new Id3v23Tag(header, reader);
                    case 4:
                        return new Id3v24Tag(header, reader);
                    case 255:
                        return null;
                    default:
                        throw new NotSupportedException("Cannot retrieve ID3 tag of version 2." + header.Version + "." + header.Revision + ".");
                }
            }
        }
    }
}
