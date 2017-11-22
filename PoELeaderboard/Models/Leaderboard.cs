using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoELeaderboard.Models
{
    public class Leaderboard
    {
        public List<Character> LeaderboardCharacters { get; set; }

        public League LeaderboardLeague { get; set; }

        public int CharacterCount { get; set; }

        public string Type { get; set; }

        public Difficulty Difficulty { get; set; }

        public DateTime LastTimeUpdated { get; set; }
    }
}
