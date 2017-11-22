using PoELeaderboard.Models;
using System;
using System.Threading.Tasks;
namespace PoELeaderboard
{
    public interface ILeaderboardUpdater
    {
        TimeSpan TimeOut { get; set; }

        Task<Leaderboard> CreateLeaderboardFromRangeAsync(LeaderboardUpdateDetails details);
        
        Task<Leaderboard> CreateLeaderboardFromRangeAsyncWithRetry(int retryCount, LeaderboardUpdateDetails details);
        
        Task<Leaderboard> GetLeaderboardFullAsync(LeaderboardUpdateDetails details);
        
        void CancelRunningUpdates();
    }
}
