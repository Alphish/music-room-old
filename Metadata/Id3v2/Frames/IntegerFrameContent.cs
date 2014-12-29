using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alphicsh.Audio.Metadata.Id3v2.Frames
{
    /// <summary>
    /// A class representing integer ID3v2 frame contents.
    /// </summary>
    public class IntegerFrameContent : TextFrameContent, IIntegerFrameContent
    {
        public Int64 Number { get; private set; }

        /// <summary>
        /// Creates a new integer frame content, based on string representation provided.
        /// </summary>
        /// <param name="text">The string representation of the integer.</param>
        public IntegerFrameContent(String text)
            : base(text)
        {
            Number = Convert.ToInt64(text);
        }
    }
}
