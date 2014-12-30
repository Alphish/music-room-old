using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Alphicsh.Interfaces.Processes;
using NAudio.Wave;

namespace Alphicsh.Audio.Analysis
{
    /// <summary>
    /// A class for samples reading process, retrieving 16-bit PCM samples as short integers.
    /// </summary>
    public class SamplesReader : BaseExtendedProcess
    {
        public Int16[][] Result { get; private set; }
        public WaveFormat Format { get; private set; }

        private BinaryReader InnerReader;
        private Func<BinaryReader, Int16> SampleGetter;
        private Int32 Channels;

        /// <summary>
        /// Creates a new samples reader from a wave stream.
        /// </summary>
        /// <param name="stream">The wave stream.</param>
        public SamplesReader(WaveStream stream)
            : base("Samples reading")
        {
            this.Result = null;
            this.Format = stream.WaveFormat;

            stream.Position = 0;
            this.InnerReader = new BinaryReader(stream);
            this.SampleGetter = this.CreateSampleGetter();
            this.Channels = stream.WaveFormat.Channels;

            this.Target = stream.Length / (Format.BitsPerSample / 8);

            this.Result = new Int16[Channels][];
            for (var i = 0; i < Channels; i++)
            {
                Result[i] = new Int16[stream.Length / stream.BlockAlign];
            }
        }

        //loads samples as short integers
        protected override void DoStep()
        {
            var intProgress = (Int32)Progress;
            var intTarget = (Int32)Target;
            var toLoad = Math.Min(intTarget - intProgress, Format.SampleRate * Channels);

            for (var i = intProgress; i < intProgress + toLoad; i += 1)
            {
                Result[i % Channels][i / Channels] = SampleGetter(InnerReader);
            }
            this.Progress += toLoad;

            if (this.Progress == this.Target)
            {
                InnerReader.Dispose();
                this.Complete();
            }
        }

        //creates sample getter depending on wave format, so that it's always represented as short integer
        protected Func<BinaryReader, Int16> CreateSampleGetter()
        {
            switch (Format.Encoding)
            {
                case WaveFormatEncoding.Pcm:
                    switch (Format.BitsPerSample)
                    {
                        case 8: return (reader) => reader.ReadByte();
                        case 16: return (reader) => reader.ReadInt16();
                        case 32: return (reader) => (Int16)(reader.ReadInt32() >> 16);
                        case 64: return (reader) => (Int16)(reader.ReadInt64() >> 48);
                        default: throw new NotSupportedException("Cannot handle Pcm encoding with " + Format.BitsPerSample + " bits per sample.");
                    }
                case WaveFormatEncoding.IeeeFloat:
                    switch (Format.BitsPerSample)
                    {
                        case 32: return (reader) => Convert.ToInt16(Int16.MaxValue * reader.ReadSingle());
                        case 64: return (reader) => Convert.ToInt16(Int16.MaxValue * reader.ReadDouble());
                        default: throw new NotSupportedException("Cannot handle IeeeFloat encoding with " + Format.BitsPerSample + " bits per sample.");
                    }
                default:
                    throw new NotSupportedException("Cannot handle wave format encoding " + Format.Encoding + ".");
            }
        }
    }
}
