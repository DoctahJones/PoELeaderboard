using PoELeaderboard.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PoELeaderboard
{
    public class LeaderboardUpdateProgressEventArgs
    {
        public LeaderboardUpdateActionState LeaderboardUpdateState { get; set; }
        public League LeagueUpdated { get; set; }
        public int UpdatesTotal { get; set; }
        public int UpdatesCompleted { get; set; }

        public LeaderboardUpdateProgressEventArgs(LeaderboardUpdateActionState leaderboardUpdateState, League leagueBeingUpdated, int updateTotal, int updatesCompleted)
        {
            LeaderboardUpdateState = leaderboardUpdateState;
            LeagueUpdated = leagueBeingUpdated;
            UpdatesTotal = updateTotal;
            UpdatesCompleted = updatesCompleted;
        }
    }
}
