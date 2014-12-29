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
            this.Channels = stream.WaveFormat.Channels;
            this.Target = stream.Length/2;

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
                Result[i % Channels][i / Channels] = InnerReader.ReadInt16();
            }
            this.Progress += toLoad;

            if (this.Progress == this.Target)
            {
                InnerReader.Dispose();
                this.Complete();
            }
        }
    }
}
