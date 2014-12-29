using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Alphicsh.Interfaces.Processes;

namespace Alphicsh.Audio.Analysis.WaveMatching
{
    /// <summary>
    /// A class for calculating Arbitrary Wave Similarity Metric (AWSM).
    /// </summary>
    public class WaveMatchingProcess : BaseExtendedProcess
    {
        protected WaveMatchingQuery Query;

        protected Int32 Current;
        protected Int32 StepSize;

        /// <summary>
        /// The AWSM calculated for each candidate loop length.
        /// </summary>
        public Dictionary<Int32, Int32> Results { get; private set; }

        /// <summary>
        /// The highest AWSM possible to obtain.
        /// </summary>
        public Int32 ScoreCap
        {
            get { return Query.ReferencePoints.Count() * Query.MatchLength * (Query.StrictScore + Query.LooseScore) * Query.Matchers.Count(); }
        }

        /// <summary>
        /// Creates a new wave matching process with a given name, based on a given query.
        /// </summary>
        /// <param name="name">The name of the process.</param>
        /// <param name="query">The wave matching query.</param>
        public WaveMatchingProcess(String name, WaveMatchingQuery query)
            : base(name)
        {
            this.Query = query;

            //setting up underlying wave matchers
            foreach (var matcher in query.Matchers)
            {
                matcher.RequireRanges(query.ReferencePoints.First(), query.ReferencePoints.Last() + query.MatchLength);
            }

            //preparing results dictionary
            Results = new Dictionary<int, int>();
            foreach (var offset in query.LoopOffsets)
            {
                Results.Add(offset, 0);
            }

            Current = 0;
            this.Target = query.LoopOffsets.Count;
            this.StepSize = (Int32)Math.Ceiling(this.Target / 100);
        }
        /// <summary>
        /// Creates a new wave matching process based on a given query.
        /// </summary>
        /// <param name="query">The wave matching query.</param>
        public WaveMatchingProcess(WaveMatchingQuery query)
            : this("Wave matching process", query)
        {
        }

        //calculates AWSM for a part of loop candidates
        protected override void DoStep()
        {
            for (var i = 0; i < this.StepSize; i++)
            {
                var candidate = Query.LoopOffsets[Current++];
                Results[candidate] = Query.Matchers.Sum(matcher => Query.ReferencePoints.Sum((Int32 point) => matcher.CompareWaves(point, candidate, Query.MatchLength, Query.StrictScore, Query.LooseScore)));

                if (Current >= Query.LoopOffsets.Count)
                {
                    this.Complete();
                    break;
                }
            }
            this.Progress = Current;
        }
    }
}
