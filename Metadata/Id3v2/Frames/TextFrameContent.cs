using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alphicsh.Audio.Metadata.Id3v2.Frames
{
    /// <summary>
    /// A class representing textual ID3v2 frame contents.
    /// </summary>
    public class TextFrameContent : ITextFrameContent
    {
        public String Text { get; private set; }

        /// <summary>
        /// Creates a new ID3v2 frame content with a given text.
        /// </summary>
        /// <param name="text">The frame content text.</param>
        public TextFrameContent(String text)
        {
            this.Text = text;
        }
    }
}
