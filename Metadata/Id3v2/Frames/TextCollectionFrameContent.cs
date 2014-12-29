using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alphicsh.Audio.Metadata.Id3v2.Frames
{
    /// <summary>
    /// A class representing multiple entries textual ID3v2 frame contents.
    /// </summary>
    public class TextCollectionFrameContent : TextFrameContent, ITextCollectionFrameContent
    {
        public IEnumerable<String> Entries { get { return this.Text.Split('/'); } }

        /// <summary>
        /// Creates a new text collection ID3v2 frame content, based on its string representation.
        /// </summary>
        /// <param name="text">The text collection string.</param>
        public TextCollectionFrameContent(String text)
            : base(text)
        {
        }
    }
}
