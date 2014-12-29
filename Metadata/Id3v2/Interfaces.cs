using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Alphicsh.Audio.Metadata.Id3v2.Frames;

namespace Alphicsh.Audio.Metadata.Id3v2
{
    /// <summary>
    /// An interface for ID3v2 tag information. 
    /// </summary>
    public interface IId3v2Tag
    {
        /// <summary>
        /// The ID3v2 tag version; cannot be 255.
        /// </summary>
        Byte Version { get; }

        /// <summary>
        /// The current version's revision; cannot be 255.
        /// </summary>
        Byte Revision { get; }

        /// <summary>
        /// Gets the tag size without header.
        /// </summary>
        Int32 Size { get; }

        /// <summary>
        /// Gets the tag size with header.
        /// </summary>
        Int32 Fullsize { get; }

        /// <summary>
        /// Gets the tag's frames.
        /// </summary>
        List<IId3v2Frame> Frames { get; }

        /// <summary>
        /// Retrieves a frame with a given identifier.
        /// </summary>
        /// <param name="id">The frame identifier.</param>
        /// <returns>The frame searched, or null if it's not found.</returns>
        IId3v2Frame Frame(String id);
    }

    /// <summary>
    /// An interface for ID3v2 tag frame information.
    /// </summary>
    public interface IId3v2Frame
    {
        /// <summary>
        /// Gets the tag the frame belongs to.
        /// </summary>
        IId3v2Tag Tag { get; }

        /// <summary>
        /// Gets the frame identifier.
        /// </summary>
        String Id { get; }
        /// <summary>
        /// Indicates if the frame is experimental or not.
        /// </summary>
        Boolean IsExperimental { get; }
        /// <summary>
        /// Gets the frame size without header.
        /// </summary>
        Int32 Size { get; }
        /// <summary>
        /// Gets the complete size of the frame.
        /// </summary>
        Int32 Fullsize { get; }

        /// <summary>
        /// Gets the content of the frame.
        /// </summary>
        IFrameContent Content { get; }
    }

}
