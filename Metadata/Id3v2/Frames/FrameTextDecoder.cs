using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alphicsh.Audio.Metadata.Id3v2.Frames
{
    /// <summary>
    /// A singleton for decoding text frames.
    /// </summary>
    public class FrameTextDecoder
    {
        /// <summary>
        /// The frame text decoder's instance.
        /// </summary>
        public static FrameTextDecoder Instance
        {
            get
            {
                if (_Instance == null)
                {
                    _Instance = new FrameTextDecoder();
                }
                return _Instance;
            }
        }
        private static FrameTextDecoder _Instance;

        private FrameTextDecoder() { }

        /// <summary>
        /// Performs decoding operation on ID3v2 tag frame content, depending ID3v2 tag version.
        /// </summary>
        /// <param name="version">The tag version for which the decoding is performed.</param>
        /// <param name="revision">The tag revision for which the decoding is performed.</param>
        /// <param name="allowNewline"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public String Decode(Byte version, Byte revision, Boolean allowNewline, Byte[] data)
        {
            return new FrameTextDecoding(version, revision, allowNewline, data).Result;
        }

        //a helper class for actually performing the decoding operation
        private class FrameTextDecoding
        {
            Byte Version;
            Byte Revision;

            Boolean AllowNewline;
            Boolean RestrictRange;

            Byte[] Data;
            Encoding UsedEncoding;
            Int32 DataOffset;
            Byte TerminationSize;

            Byte[] TextData;
            public String Result;

            //creates a new decoding operation
            public FrameTextDecoding(Byte version, Byte revision, Boolean allowNewline, Byte[] data)
            {
                this.Version = version;
                this.Revision = revision;
                this.AllowNewline = allowNewline;
                this.Data = data;

                this.DoDecode();
            }

            //performs the decoding so that its result would be created
            private void DoDecode()
            {
                this.FindEncoding();
                this.FindTextData();

                if (RestrictRange)
                {
                    Byte badChar = TextData.FirstOrDefault(b => b < 0x20 && b > 0x00 && (b != 0x0A || !AllowNewline));

                    if (badChar > 0)
                    {
                        throw new FormatException(
                            String.Format("ISO-8859-1 encoded " + (AllowNewline ? "full " : "") + "text string cannot have ${0} character.", badChar.ToString("X2"))
                            );
                    }
                }

                this.Result = UsedEncoding.GetString(TextData);                
            }

            //detects the encoding based on initial bytes
            private void FindEncoding()
            {
                Byte code = Data[0];
                this.DataOffset = 1;
                this.RestrictRange = false;

                if ((Version == 3 && code >= 2) || (Version == 4 && code >= 4))
                {
                    throw new FormatException(
                        String.Format("ID3v2.{0}.{1} doesn't allow encoding with code ${2}.", Version, Revision, code.ToString("X2"))
                        );
                }

                switch (code)
                {
                    //ISO-8859-1
                    case 0:
                        UsedEncoding = Encoding.GetEncoding("ISO-8859-1");
                        TerminationSize = 1;
                        RestrictRange = true;
                        break;

                    //UTF16 Unicode with BOM
                    case 1:
                        DataOffset += 2;  //actual data starts 2 bytes later, due to BOM usage
                        TerminationSize = 2;
                        if (Data[1] == 0xFF && Data[2] == 0xFE)
                        {
                            UsedEncoding = Encoding.Unicode;
                            break;
                        }
                        if (Data[1] == 0xFE && Data[2] == 0xFF)
                        {
                            UsedEncoding = Encoding.BigEndianUnicode;
                            break;
                        }
                        throw new FormatException("Text frame with UFT16-BOM encoding didn't start with BOM.");
                
                    //UTF16 Big-Endian
                    case 2:
                        UsedEncoding = Encoding.BigEndianUnicode;
                        TerminationSize = 2;
                        break;

                    //UTF8
                    case 3:
                        UsedEncoding = Encoding.UTF8;
                        TerminationSize = 1;
                        break;
                }
            }

            //finds text data until running out of bytes, or meeting string termination character
            private void FindTextData()
            {
                var offset = DataOffset;

                //repeat until stated otherwise
                while (offset < Data.Length)
                {
                    //check if any termination byte candidates is non-zero
                    Boolean allZeroes = true;
                    for (var i = offset; i<offset + TerminationSize; i++)
                    {
                        if (Data[i] > 0)
                        {
                            allZeroes = false;
                            break;
                        }
                    }
                    //break if all are zeroes
                    if (allZeroes)
                    {
                        break;
                    }

                    //increase accumulated offset
                    offset += TerminationSize;
                }

                this.TextData = Data.Skip(DataOffset).Take(offset - DataOffset).ToArray();
            }
        }
    }
}
