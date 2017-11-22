using PoELeaderboard.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PoELeaderboard
{
    public interface ILeagueListUpdater
    {
        Task<List<League>> GetLeagueList();
    }
}
