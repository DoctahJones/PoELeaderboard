using PoELeaderboard.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoELeaderboard
{
    public class LeaderboardUpdateEventArgs : EventArgs
    {
        public League LeagueUpdated { get; set; }

        public DateTime TimeUpdated { get; set; }

        public string TypeUpdated { get; set; }

        public int DifficultyUpdated { get; set; }

       

        public LeaderboardUpdateEventArgs(League leagueUpdated, DateTime timeUpdated, string typeUpdated, int difficultyUpdated)
        {
            LeagueUpdated = leagueUpdated;
            TypeUpdated = typeUpdated;
            TimeUpdated = timeUpdated;
            DifficultyUpdated = difficultyUpdated;
        }

    }
}
