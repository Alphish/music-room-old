using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Alphicsh.Interfaces.Processes;
using NAudio.Wave;

namespace Alphicsh.Audio.Analysis
{
    /// <summary>
    /// A class for cutting fade-out from samples array.
    /// </summary>
    public class SamplesTrimmer : BaseExtendedProcess
    {
        public Int16[][] Result { get; private set; }
        public WaveFormat Format { get; private set; }

        private List<Int32> SecondsRanges;

        private Int16[][] SourceSamples;

        /// <summary>
        /// Creates a new samples trimmer, passing a samples array and a given wave format.
        /// </summary>
        /// <param name="samples">The samples to trim fade-out from.</param>
        /// <param name="format">The wave format.</param>
        public SamplesTrimmer(Int16[][] samples, WaveFormat format)
            : base("Samples trimming")
        {
            this.SecondsRanges = new List<Int32>();

            this.Result = null;
            this.SourceSamples = samples;
            this.Format = format;

            this.Target = samples[0].Length;
        }

        //loads samples seconds and calculates peak-to-peak value per second
        protected override void DoStep()
        {
            var intProgress = (Int32)Progress;
            var intTarget = (Int32)Target;
            var toCheck = Math.Min(intTarget - intProgress, Format.SampleRate);

            var segments = SourceSamples.Select((channel) => new ArraySegment<Int16>(channel, intProgress, toCheck));
            var min = segments.Min(segment => segment.Min());
            var max = segments.Max(segment => segment.Max());
            var range = max - min;

            SecondsRanges.Add(range);

            Progress += toCheck;

            if (this.Progress == this.Target)
            {
                this.FindResult();
                this.Complete();
            }
        }

        //finds the seconds to preserve, trimming the rest
        private void FindResult()
        {
            Int32 maxRange = SecondsRanges.Max();
            Int32 limit = 4 * maxRange / 5;         //reaching ~80% of max peak-to-peak value is considered good enough

            //number of seconds to copy; "2" is added to leave a bit of otherwise trimmed fragment
            Int32 copiedSeconds = SecondsRanges.Count + 2;

            //decreasing number of seconds to be copied
            for (var i=SecondsRanges.Count-1; i>=0; i--)
            {
                //the acceptable peak-to-peak value has been reached
                if (SecondsRanges[i] >= limit)
                {
                    break;
                }

                //the following peak-to-peak value is larger than the current one
                if (i < SecondsRanges.Count-1 && SecondsRanges[i] < SecondsRanges[i+1])
                {
                    copiedSeconds++;
                    break;
                }
                copiedSeconds--;
            }

            //trimming, if needed
            if (copiedSeconds >= SecondsRanges.Count)
            {
                this.Result = SourceSamples;
            }
            else
            {
                this.Result = SourceSamples.Select(
                    channel =>
                    {
                        var result = new Int16[copiedSeconds * Format.SampleRate];
                        Array.Copy(channel, result, result.Length);
                        return result;
                    }
                    ).ToArray();
            }
        }

    }
}
