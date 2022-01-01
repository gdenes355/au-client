using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.nwdata
{
    [Serializable]
    public class VoteState
    {
        public UInt64 started;
        public List<Vote> votes;

        [Serializable]
        public class Vote
        {
            public int who;
            public int whom;
        }
    }
}
