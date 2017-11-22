using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PoELeaderboard.Models;

namespace PoELeaderboard
{
    public static class Constants
    {
        public static IReadOnlyCollection<Difficulty> GetDifficulties()
        {
            return new List<Difficulty> { 
                new Difficulty{ Name="Normal", IntValue= 1 }, 
                new Difficulty{ Name="Cruel", IntValue= 2 },
                new Difficulty{ Name="Merciless", IntValue= 3 },
                new Difficulty{ Name="Eternal", IntValue= 4 }
            }.AsReadOnly();
        }

        public static IReadOnlyCollection<string> GetLeaderboardTypes()
        {
            return new List<string>{
                "League",
                "Labyrinth"
            }.AsReadOnly();
        }

    }
}
