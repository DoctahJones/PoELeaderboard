using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoELeaderboard.Models
{
    public class Character
    {
        public string CharacterName { get; set; }

        public string AccountName { get; set; }

        public int Rank { get; set; }

        public long Level { get; set; }

        public string CharacterClass { get; set; }

        public long Experience { get; set; }

        public bool Online { get; set; }

        public bool Dead { get; set; }

        public int Challenges { get; set; }

        public int TimeLab { get; set; }

        public string Id { get; set; }

        public static Character NewDummyCharacter()
        {
            return new Character { CharacterName = "New", CharacterClass = "New" };
        }
    }
}
 