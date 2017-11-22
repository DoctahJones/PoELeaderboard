using PoELeaderboard.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoELeaderboard
{
    public class LeaderboardUpdateDetails
    {
        public League League { get; set; }

        public int Limit { get; set; }

        public int Offset { get; set; }

        public string Type { get; set; }

        public int Difficulty { get; set; }

        public LeaderboardUpdateDetails(League league, int limit, int offset, string type, int difficulty)
        {
            League = league;
            Limit = limit;
            Offset = offset;
            Type = type;
            Difficulty = difficulty;
        }

        public LeaderboardUpdateDetails(League league, string type = "league", int difficulty = 1)
        {
            League = league;
            Type = type;
            Difficulty = difficulty;
            Limit = 200;
            Offset = 0;
        }


    }
}
