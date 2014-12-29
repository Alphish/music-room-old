using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NAudio.Wave;

namespace Alphicsh.Audio.Streaming
{
    /// <summary>
    /// Stream for playing soundtrack between two points.
    /// </summary>
    public class LoopStream : WaveStream
    {
        /// <summary>
        /// The underlying wave stream.
        /// </summary>
        private WaveStream SourceStream;
        
        /// <summary>
        /// Loop beginning position; returned to once the loop is reached.
        /// </summary>
        public Int64 LoopStart { get; protected set; }
        /// <summary>
        /// Loop end position; when reached, stream returns to the loop beginning.
        /// </summary>
        public Int64 LoopEnd { get; protected set; }
        /// <summary>
        /// Toggles the looping on and off.
        /// </summary>
        public Boolean IsLooping { get; set; }

        /// <summary>
        /// Creates a new loop stream, with given starting and ending points (provided as sample indices or byte positions).
        /// </summary>
        /// <param name="sourceStream">The source stream to loop.</param>
        /// <param name="loopStart">The beginning point of the loop.</param>
        /// <param name="loopEnd">The ending point of the loop.</param>
        /// <param name="useBlockAlign">Whether looping points should be multiplied by block alignment (sample indices) or not (byte positions).</param>
        public LoopStream(WaveStream sourceStream, Int64 loopStart, Int64 loopEnd, Boolean useBlockAlign)
        {
            this.SourceStream = sourceStream;
            this.IsLooping = true;

            if (loopStart >= loopEnd)
                throw new ArgumentException("Loop beginning cannot occur after loop end.");

            this.LoopStart = loopStart * (useBlockAlign ? sourceStream.BlockAlign : 1);
            this.LoopEnd = loopEnd * (useBlockAlign ? sourceStream.BlockAlign : 1);
        }
        /// <summary>
        /// Creates a new loop stream, with given starting and ending points (provided as sample indices).
        /// </summary>
        /// <param name="sourceStream">The source stream to loop.</param>
        /// <param name="loopStart">The beginning point of the loop.</param>
        /// <param name="loopEnd">The ending point of the loop.</param>
        public LoopStream(WaveStream sourceStream, Int64 loopStart, Int64 loopEnd)
            : this(sourceStream, loopStart, loopEnd, true)
        {
        }

        public override WaveFormat WaveFormat
        {
            get { return SourceStream.WaveFormat; }
        }
        public override Int64 Length
        {
            get { return SourceStream.Length; }
        }
        public override Int64 Position
        {
            get { return SourceStream.Position; }
            set { SourceStream.Position = value; }
        }

        public override Int32 Read(Byte[] buffer, Int32 offset, Int32 count)
        {
            Int32 totalBytesRead = 0;
            Int32 bytesRead = -1;

            while (totalBytesRead < count || (!IsLooping && bytesRead == 0))
            {
                //checking how many bytes are left till reaching the loop end
                Int32 tillNextLoop = (Int32)(IsLooping ? LoopEnd - Position : Length - Position);
                if (tillNextLoop <= 0 && IsLooping)
                {
                    //looping itself
                    Position = LoopStart;
                    continue;
                }
                //reading as many bytes as possible, but no more than number remaining till the loop end
                Int32 toRead = Math.Min(count - totalBytesRead, tillNextLoop);
                bytesRead = SourceStream.Read(buffer, offset + totalBytesRead, toRead);
                totalBytesRead += bytesRead;
            }

            return totalBytesRead;
        }

        protected override void Dispose(Boolean disposing)
        {
            SourceStream.Dispose();
            base.Dispose(disposing);
        }
    }
}
