using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Alphicsh.Interfaces.Processes;
using Alphicsh.Audio.Analysis.WaveMatching;

namespace Alphicsh.Audio.Analysis.LoopDetection
{
    /// <summary>
    /// A class for query planning process. Despite being technically an extended process, it has only single step. To be used as a part of pipeline.
    /// </summary>
    public class QueryPlanner : BaseExtendedProcess
    {
        /// <summary>
        /// The queries to execute.
        /// </summary>
        public IEnumerable<WaveMatchingQuery> Queries { get { return _Queries; } }
        private List<WaveMatchingQuery> _Queries;

        protected IEnumerable<WavePatternMatcher> Matchers;

        protected Int32 ReferenceMin;
        protected Int32 ReferenceMax;
        protected Int32 OffsetMin;
        protected Int32 OffsetMax;

        protected Int32 StrictScore;
        protected Int32 LooseScore;

        /// <summary>
        /// Creates a new query planner, based on parameters given.
        /// </summary>
        /// <param name="name">The name of the process.</param>
        /// <param name="matchers">Wave matchers given.</param>
        /// <param name="referenceMin">The earliest reference point.</param>
        /// <param name="referenceMax">The latest reference point.</param>
        /// <param name="offsetMin">The minimum expected loop offset.</param>
        /// <param name="offsetMax">The maximum expected loop offset.</param>
        /// <param name="strictScore">The score added for strictly matching the wave.</param>
        /// <param name="looseScore">The score added for loosely matching the wave.</param>
        public QueryPlanner(String name, IEnumerable<WavePatternMatcher> matchers, Int32 referenceMin, Int32 referenceMax, Int32 offsetMin, Int32 offsetMax, Int32 strictScore, Int32 looseScore)
            : base(name)
        {
            this._Queries = new List<WaveMatchingQuery>();

            this.Matchers = new List<WavePatternMatcher>(matchers);

            this.ReferenceMin = referenceMin;
            this.ReferenceMax = referenceMax;
            this.OffsetMin = offsetMin;
            this.OffsetMax = offsetMax;

            this.StrictScore = strictScore;
            this.LooseScore = looseScore;
        }

        /// <summary>
        /// Adds a new query to the list.
        /// </summary>
        /// <param name="matchCount">Number of matching points.</param>
        /// <param name="matchLength">Length of an individual match.</param>
        /// <param name="mono">Whether the search should be carried out on all channels or only the first one.</param>
        public void AddQuery(Int32 matchCount, Int32 matchLength, Boolean mono)
        {
            if (this._Queries.Any())
            {
                this._Queries.Add(new WaveMatchingQuery(
                    mono ? Matchers.Take(1) : Matchers,
                    ReferenceMin, ReferenceMax, matchCount, matchLength,
                    null,
                    StrictScore, LooseScore
                    ));
            }
            else
            {
                this._Queries.Add(new WaveMatchingQuery(
                    mono ? Matchers.Take(1) : Matchers,
                    ReferenceMin, ReferenceMax, matchCount, matchLength,
                    OffsetMin, OffsetMax,
                    StrictScore, LooseScore
                    ));
            }
        }

        //completes the process so that its result can be grabbed
        protected override void DoStep()
        {
            Complete();
        }
    }
}
