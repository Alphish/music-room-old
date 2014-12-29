using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alphicsh.Audio.Metadata.Id3v2.Frames
{
    /// <summary>
    /// A general interface for ID3v2 frame content.
    /// </summary>
    public interface IFrameContent
    {
    }

    /// <summary>
    /// An interface for textual ID3v2 frame content.
    /// </summary>
    public interface ITextFrameContent : IFrameContent
    {
        /// <summary>
        /// The frame's contained text.
        /// </summary>
        String Text { get; }
    }
    /// <summary>
    /// An interface for integer ID3v2 frame content.
    /// </summary>
    public interface IIntegerFrameContent : ITextFrameContent
    {
        /// <summary>
        /// The frame's contained number.
        /// </summary>
        Int64 Number { get; }
    }
    /// <summary>
    /// An interface for multiple entries textual ID3v2 frame content.
    /// </summary>
    public interface ITextCollectionFrameContent : ITextFrameContent
    {
        /// <summary>
        /// The frame's entries.
        /// </summary>
        IEnumerable<String> Entries { get; }
    }
    /// <summary>
    /// An interface for ID3v2 URL frame content.
    /// </summary>
    public interface IUrlFrameContent : IFrameContent
    {
        /// <summary>
        /// The frame's contained URL.
        /// </summary>
        String Url { get; }
    }

    /// <summary>
    /// An interface for binary ID3v2 frame content.
    /// </summary>
    public interface IBinaryFrameContent : IFrameContent
    {
        /// <summary>
        /// The frame's binary data.
        /// </summary>
        Byte[] Data { get; }
    }
    /// <summary>
    /// An interface for picture ID3v2 frame content.
    /// </summary>
    public interface IPictureFrameContent : IBinaryFrameContent
    {
        /// <summary>
        /// The picture's MIME type.
        /// </summary>
        String Mime { get; }
    }
}
