using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PoELeaderboard.Models;
using PoELeaderboard.APIRequests;

namespace PoELeaderboard
{
    /// <summary>
    /// Class that holds a list of all the currently being tracked leaderboards, therefore if multiple viewmodels are tracking the same leaderboard
    /// they can both be updated at the same time. When a leaderboard is updated we send off web requests and collate them into the collection here.
    /// The viewmodels can subscribe to the event LeaderboardUpdate and check if that is for the leaderboard they are displaying.
    /// </summary>
    public class LeaderboardCollection
    {
        //The delegate for the event which can be subscribed to to receive updates about leaderboards.
        public delegate void LeaderboardUpdateHandler(Object sender, LeaderboardUpdateEventArgs e);
        //The event which can be subscribed to to be notified when the leaderboard collection has been updated.
        public event LeaderboardUpdateHandler LeaderboardUpdate;
        //The collection of leaderboards.
        private List<Leaderboard> leaderboards;
        //The class used to update the leaderboards with more recent versions.
        private ILeaderboardUpdater leaderboardUpdater;

        public LeaderboardCollection(ILeaderboardUpdater updater)
        {
            leaderboards = new List<Leaderboard>();
            leaderboardUpdater = updater;
        }

        public async Task GetFullLeaderboardFromWeb(League league, string type="league", int difficulty= 1)
        {
            var leaderboard = await leaderboardUpdater.GetLeaderboardFullAsync(new LeaderboardUpdateDetails(league, type, difficulty));

            if (leaderboard != null && leaderboard.LeaderboardCharacters != null)
            {
                var outdatedLeaderboard = GetLeaderboard(league, type, difficulty);
                if (outdatedLeaderboard != null)
                {
                    leaderboards.Remove(outdatedLeaderboard);
                }
                leaderboard.LeaderboardCharacters = leaderboard.LeaderboardCharacters.OrderBy(c => c.Rank).ToList();
                leaderboard.LastTimeUpdated = DateTime.Now;
                leaderboards.Add(leaderboard);
                OnLeaderboardUpdated(leaderboard.LeaderboardLeague, type, difficulty); 
            }
        }

        public Leaderboard GetLeaderboard(League league, string type, int difficulty)
        {
            var leaguesWithNameAndType = leaderboards.Where(l => l.LeaderboardLeague.Name == league.Name
                    && String.Equals(l.Type, type, StringComparison.InvariantCultureIgnoreCase));

            if (String.Equals(type, "league", StringComparison.InvariantCultureIgnoreCase))
            {
                return leaguesWithNameAndType.FirstOrDefault();
            }
            else if (String.Equals(type, "labyrinth", StringComparison.InvariantCultureIgnoreCase))
            {
                return leaguesWithNameAndType.FirstOrDefault(l => l.Difficulty.IntValue == difficulty);
            }
            else return default(Leaderboard);
        }

        private void OnLeaderboardUpdated(League leagueUpdated, string typeUpdated, int difficulty)
        {
            if (LeaderboardUpdate != null)
            {
                LeaderboardUpdate(this, new LeaderboardUpdateEventArgs(leagueUpdated, DateTime.Now, typeUpdated, difficulty));
            }
        }

        public void CancelUpdates()
        {
            this.leaderboardUpdater.CancelRunningUpdates();
        }

    }
}
