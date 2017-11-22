using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoELeaderboard
{
    public enum LeaderboardUpdateActionState
    {
        FETCHING_DETAILS, UPDATING, RETRYING, COMPLETED, CANCELLED
    }
}
