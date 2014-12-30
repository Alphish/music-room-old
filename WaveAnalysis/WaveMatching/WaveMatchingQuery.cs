using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alphicsh.Audio.Analysis.WaveMatching
{
    /// <summary>
    /// A class for parameterizing wave matching processes.
    /// </summary>
    public class WaveMatchingQuery
    {
        /// <summary>
        /// Underlying wave pattern matchers to calculate AWSM with.
        /// </summary>
        public IEnumerable<WavePatternMatcher> Matchers { get; protected set; }

        /// <summary>
        /// Reference points to check patterns from.
        /// </summary>
        public IEnumerable<Int32> ReferencePoints { get; private set; }
        /// <summary>
        /// Individual matches length.
        /// </summary>
        public Int32 MatchLength { get; protected set; }

        /// <summary>
        /// Candidates for loop offsets.
        /// </summary>
        public IList<Int32> LoopOffsets { get; private set; }

        /// <summary>
        /// Loop offsets multipliers.
        /// </summary>
        public List<Int32> Echoes { get; set; }

        /// <summary>
        /// The score added for strictly fitting in a matcher's area.
        /// </summary>
        public Int32 StrictScore { get; protected set; }
        /// <summary>
        /// The score added for loosely fitting in a matcher's area.
        /// </summary>
        public Int32 LooseScore { get; protected set; }

        //gosh, it sure has a lot of parameters!
        public WaveMatchingQuery(IEnumerable<WavePatternMatcher> matchers, Int32 referenceStart, Int32 referenceEnd, Int32 matchCount, Int32 matchLength, Int32 loopMin, Int32 loopMax, Int32 strictScore, Int32 looseScore)
        {
            this.Matchers = new List<WavePatternMatcher>(matchers);
            this.SetupReferencePoints(referenceStart, referenceEnd, matchCount, matchLength);

            if (loopMin != 0)
            {
                this.InjectOffsets(loopMin, loopMax);
            }

            this.StrictScore = strictScore;
            this.LooseScore = looseScore;

            this.Echoes = new List<Int32> { 1 };
        }

        //and it doesn't seem to be much different in that regard, either
        public WaveMatchingQuery(IEnumerable<WavePatternMatcher> matchers, Int32 referenceStart, Int32 referenceEnd, Int32 matchCount, Int32 matchLength, IEnumerable<Int32> offsets, Int32 strictScore, Int32 looseScore)
        {
            this.Matchers = new List<WavePatternMatcher>(matchers);
            this.SetupReferencePoints(referenceStart, referenceEnd, matchCount, matchLength);

            if (offsets != null)
            {
                this.LoopOffsets = new List<Int32>(offsets);
            }

            this.StrictScore = strictScore;
            this.LooseScore = looseScore;

            this.Echoes = new List<Int32> {1};
        }
        //perhaps I shouldn't put it all in a constructor? Hmm...

        /// <summary>
        /// Calculates reference points based on reference range and matches parameters.
        /// </summary>
        /// <param name="referenceMin">The start of reference range.</param>
        /// <param name="referenceMax">The end of reference range.</param>
        /// <param name="matchCount">The matches count.</param>
        /// <param name="matchLength">An individual match length.</param>
        private void SetupReferencePoints(Int32 referenceMin, Int32 referenceMax, Int32 matchCount, Int32 matchLength)
        {
            Int32 refRange = referenceMax - referenceMin;
            if (refRange < matchLength)
            {
                throw new ArgumentException("Reference range cannot be smaller than match length.");
            }
            this.MatchLength = matchLength;

            if (matchCount == 1)
            {
                this.ReferencePoints = new List<Int32>() { referenceMin + (refRange - matchLength) / 2 };
                return;
            }

            Int32 startsRange = refRange - matchLength;
            var points = new List<Int32>();
            for (var i = 0; i < matchCount; i++)
            {
                points.Add(referenceMin + (startsRange * i) / (matchCount - 1));
            }
            this.ReferencePoints = points;
        }

        /// <summary>
        /// Inserts loop candidates in a query.
        /// </summary>
        /// <param name="offsets">The offset candidates.</param>
        public void InjectOffsets(IEnumerable<Int32> offsets)
        {
            if (this.LoopOffsets != null)
            {
                throw new InvalidOperationException("The wave matching query already has offsets setup.");
            }
            this.LoopOffsets = new List<Int32>(offsets);
        }

        /// <summary>
        /// Insert a range of loop candidates in a query.
        /// </summary>
        /// <param name="loopMin">Minimum expected loop offset.</param>
        /// <param name="loopMax">Maximum expected loop offset.</param>
        public void InjectOffsets(Int32 loopMin, Int32 loopMax)
        {
            if (this.LoopOffsets != null)
            {
                throw new InvalidOperationException("The wave matching query already has offsets setup.");
            }
            this.LoopOffsets = Enumerable.Range(loopMin, loopMax - loopMin).ToList();
        }
    }

}
