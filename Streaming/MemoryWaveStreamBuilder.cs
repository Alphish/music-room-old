using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Alphicsh.Interfaces.Processes;
using NAudio.Wave;

namespace Alphicsh.Audio.Streaming
{
    /// <summary>
    /// A class for wave stream buffering process.
    /// </summary>
    public class MemoryWaveStreamBuilder : BaseExtendedProcess
    {
        private WaveStream Source;
        /// <summary>
        /// Gets the wave data read.
        /// </summary>
        public Byte[] ReadData { get; private set; }

        /// <summary>
        /// Indicates whether all bytes have been read or not.
        /// </summary>
        public Boolean AllBytesRead { get; private set; }
        /// <summary>
        /// The in-memory wave stream built.
        /// </summary>
        public WaveStream Result { get; private set; }

        /// <summary>
        /// Creates a new wave stream buffering process.
        /// </summary>
        /// <param name="name">The name of the process.</param>
        /// <param name="source">The wave stream to buffer.</param>
        public MemoryWaveStreamBuilder(String name, WaveStream source)
            : base(name)
        {
            this.Source = source;

            this.Target = source.Length;
            this.ReadData = new Byte[source.Length];

            this.AllBytesRead = false;
            this.Result = null;
            this.ProcessFinished += (sender, e) => this.Source.Dispose();
        }
        /// <summary>
        /// Creates a new wave stream buffering process.
        /// </summary>
        /// <param name="source">The wave stream to buffer.</param>
        public MemoryWaveStreamBuilder(WaveStream source)
            : this("Memory wave stream building", source)
        {
        }
        /// <summary>
        /// Creates a new wave stream buffering process.
        /// </summary>
        /// <param name="name">The name of the process.</param>
        /// <param name="filename">The file to create the wave stream from.</param>
        public MemoryWaveStreamBuilder(String name, String filename)
            : this(name, WaveStreamFactory.Create(filename))
        {
        }
        /// <summary>
        /// Creates a new wave stream buffering process.
        /// </summary>
        /// <param name="filename">The file to create the wave stream from.</param>
        public MemoryWaveStreamBuilder(String filename)
            : this("Memory wave stream building", filename)
        {
        }

        //performs a buffering step, or creates the in-memory stream if all bytes are read
        protected override void DoStep()
        {
            if (!AllBytesRead)
            {
                this.ReadBytes();
            }
            else
            {
                this.CreateResult();
            }
        }

        //reads the bytes from the source stream
        private void ReadBytes()
        {
            Int32 abps = this.Source.WaveFormat.AverageBytesPerSecond;
            Int32 allBytesRead = (Int32)Progress;
            Int32 totalBytesRead = 0;
            Int32 bytesRead;

            while ((bytesRead = Source.Read(ReadData, allBytesRead, abps - totalBytesRead)) > 0)
            {
                totalBytesRead += bytesRead;
                allBytesRead += bytesRead;
            }
            Progress += totalBytesRead;

            if (this.Progress >= this.Target)
            {
                this.Target = 0;
                this.AllBytesRead = true;
            }
        }
        //creates an in-memory wave stream from the bytes gathered
        private void CreateResult()
        {
            this.Result = new RawSourceWaveStream(new MemoryStream(ReadData), Source.WaveFormat);
            this.Complete();
        }
    }
}
