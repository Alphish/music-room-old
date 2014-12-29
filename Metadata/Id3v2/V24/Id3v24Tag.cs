using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alphicsh.Audio.Metadata.Id3v2.V24
{
    //it's a work in progress
    public class Id3v24Tag : Id3v2Tag
    {
        public override int Fullsize
        {
            get { return this.Size + 10; }
        }

        public Id3v24Tag(Id3v2TagHeader header, BinaryReader reader)
            : base(header)
        {

        }
    }
}
