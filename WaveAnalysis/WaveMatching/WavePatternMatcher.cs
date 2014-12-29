using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alphicsh.Audio.Analysis.WaveMatching
{
    /// <summary>
    /// A class for building wave pattern and checking actual waves against it.
    /// </summary>
    public class WavePatternMatcher
    {
        /// <summary>
        /// An enmerable describing wave changeability.
        /// </summary>
        public enum DifChain { Descending = 0, LateToAsc = 1, DescVariable = 2, EarlyToAsc = 3, EarlyToDesc = 4, AscVariable = 5, LateToDesc = 6, Ascending = 7 };

        //the offset of the currently found pattern
        Int32 Offset;

        /// <summary>
        /// The pattern calculated, given as list of strict and loose ranges.
        /// </summary>
        public List<WavePatternRange> SampleRanges { get; private set; }
        /// <summary>
        /// The samples to compare.
        /// </summary>
        public Int16[] Samples;

        /// <summary>
        /// Creates a new wave pattern matcher, based on samples array given.
        /// </summary>
        /// <param name="samples">The samples array.</param>
        public WavePatternMatcher(Int16[] samples)
        {
            this.Samples = samples;
            this.SampleRanges = new List<WavePatternRange>();
            this.Offset = -1;
        }

        /// <summary>
        /// Calculates a pattern ranges for a given fragment, if needed.
        /// </summary>
        /// <param name="index">The index where the pattern should start.</param>
        /// <param name="count">The number of pattern ranges to include.</param>
        public void RequireRanges(Int32 index, Int32 count)
        {
            //the first range ever
            if (Offset == -1)
            {
                Offset = index;
                this.SampleRanges.InsertRange(0, _FindRanges(index, index + count));
                return;
            }

            //not the first range ever
            if (index + count > Offset + SampleRanges.Count)
            {
                this.SampleRanges.AddRange(_FindRanges(Offset + SampleRanges.Count, index + count));
            }
            if (index < Offset)
            {
                this.SampleRanges.InsertRange(0, _FindRanges(index, Offset));
            }
        }
        //actually calculates them ranges
        private IEnumerable<WavePatternRange> _FindRanges(Int32 startIndex, Int32 endIndex)
        {
            //preparation
            var enumerator = Samples.Skip(startIndex - 1).Take((endIndex - startIndex) + 3).GetEnumerator();
            Int16 left = 0;
            enumerator.MoveNext();

            var midLeft = enumerator.Current;
            var dLeft = midLeft - left;
            enumerator.MoveNext();

            var midRight = enumerator.Current;
            var dMid = midRight - midLeft;
            enumerator.MoveNext();

            var right = enumerator.Current;
            var dRight = right - midRight;

            var chain = (DifChain)((dMid >= 0 ? 2 : 0) + (dRight >= 0 ? 1 : 0));

            var result = new List<WavePatternRange>(endIndex - startIndex);

            //adding the ranges to the pattern
            while (enumerator.MoveNext())
            {
                left = midLeft;
                dLeft = dMid;
                midLeft = midRight;
                dMid = dRight;
                midRight = right;
                right = enumerator.Current;
                dRight = right - midRight;

                chain = (DifChain)(2 * ((Int32)chain % 4) + (dRight >= 0 ? 1 : 0));

                switch (chain)
                {
                    case DifChain.Descending:
                        result.Add(new WavePatternRange(midRight, midLeft, right, left));
                        break;
                    case DifChain.LateToAsc:
                    case DifChain.EarlyToAsc:
                        result.Add(new WavePatternRange(Math.Min(midLeft + dLeft, midRight - dRight), Math.Max(midLeft, midRight), Math.Min(midLeft + 2 * dLeft, midRight - 2 * dRight), Math.Max(left, right)));
                        break;
                    case DifChain.DescVariable:
                        result.Add(new WavePatternRange(midLeft + dLeft, midRight - dRight, midLeft + 2 * dLeft, midRight - 2 * dRight));
                        break;

                    case DifChain.Ascending:
                        result.Add(new WavePatternRange(midLeft, midRight, left, right));
                        break;
                    case DifChain.LateToDesc:
                    case DifChain.EarlyToDesc:
                        result.Add(new WavePatternRange(Math.Min(midLeft, midRight), Math.Max(midLeft + dLeft, midRight - dRight), Math.Min(left, right), Math.Max(midLeft + 2 * dLeft, midRight - 2 * dRight)));
                        break;
                    case DifChain.AscVariable:
                        result.Add(new WavePatternRange(midRight - dRight, midLeft + dLeft, midRight - 2 * dRight, midLeft + 2 * dLeft));
                        break;
                }
            }

            return result;
        }

        /*
        public Int32 MassCompareWaves(Int32 referenceOffset, Int32 checkedOffset, Int32 checks, Int32 countPerCheck, Int32 offsetPerCheck, Int32 strictBonus, Int32 looseBonus)
        {
            var result = Int32.MaxValue;
            for (var i=0; i<checks; i++)
            {
                result = Math.Min(result,
                    CompareWaves(referenceOffset + i * offsetPerCheck, checkedOffset + i * offsetPerCheck, countPerCheck, strictBonus, looseBonus)
                    );
            }
            return result;
        }
         */

        /// <summary>
        /// Calculates Arbitrary Wave Similarity Metric (AWSM) for a given reference point and hypothetical loop length.
        /// </summary>
        /// <param name="referenceIndex">The point to start comparing the pattern from.</param>
        /// <param name="comparedOffset">The loop length to check.</param>
        /// <param name="length">The length of matching.</param>
        /// <param name="strictScore">The score added for fitting in a strict range.</param>
        /// <param name="looseScore">The score added for fitting in a loose range.</param>
        /// <returns></returns>        
        public Int32 CompareWaves(Int32 referenceIndex, Int32 comparedOffset, Int32 length, Int32 strictScore, Int32 looseScore)
        {
            var result = 0;
            var rangeOffset = referenceIndex - Offset;
            var comparedIndex = referenceIndex + comparedOffset;

            for (var i = 0; i < length; i++)
            {
                var row = SampleRanges[rangeOffset + i];
                var sample = Samples[comparedIndex + i];
                if (sample >= row.StrictMin && sample <= row.StrictMax)
                    result += strictScore;
                if (sample >= row.LooseMin && sample <= row.LooseMax)
                    result += looseScore;
            }
            return result;
        }
    }

    /// <summary>
    /// A structure determining acceptable sample range in a wave pattern.
    /// </summary>
    public struct WavePatternRange
    {
        /// <summary>
        /// The minimum of strict range.
        /// </summary>
        public Int16 StrictMin { get; private set; }
        /// <summary>
        /// The maximum of strict range.
        /// </summary>
        public Int16 StrictMax { get; private set; }

        /// <summary>
        /// The minimum of loose range.
        /// </summary>
        public Int16 LooseMin { get; private set; }
        /// <summary>
        /// The maximum of loose range.
        /// </summary>
        public Int16 LooseMax { get; private set; }

        /// <summary>
        /// Creates a wave pattern range, based on ranges given.
        /// </summary>
        /// <param name="strictMin">Strict range minimum.</param>
        /// <param name="strictMax">Strict range maximum.</param>
        /// <param name="looseMin">Loose range minimum.</param>
        /// <param name="weakMax">Loose range maximum.</param>
        public WavePatternRange(Int32 strictMin, Int32 strictMax, Int32 looseMin, Int32 looseMax)
            : this()
        {
            this.StrictMin = ClampToInt16(strictMin);
            this.StrictMax = ClampToInt16(strictMax);

            this.LooseMin = ClampToInt16(looseMin);
            this.LooseMax = ClampToInt16(looseMax);
        }
        /// <summary>
        /// Clamps the number to In16 range.
        /// </summary>
        /// <param name="num">The number to clamp.</param>
        /// <returns>A proper short integer.</returns>
        private Int16 ClampToInt16(Int32 num)
        {
            return (Int16)(num < Int16.MinValue ? Int16.MinValue : (num > Int16.MaxValue ? Int16.MaxValue : num));
        }
    }
}
