using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PoELeaderboard.APIRequests;
using PoELeaderboard.Models;
using System.Threading;

namespace PoELeaderboard
{
    public class LeagueListUpdater : ILeagueListUpdater
    {

        private RateLimitedAPIRequestDispatcher requestDispatcher;

        public LeagueListUpdater(RateLimitedAPIRequestDispatcher requestDispatcher)
        {
            this.requestDispatcher = requestDispatcher;
        }

        public async Task<List<League>> GetLeagueList()
        {
            string jsonString = "";

            var l = new LeagueAPIRequest(new APIRequestSenderJSON());
            jsonString = await requestDispatcher.MakeAPIRequestAsyncPriority(l, CancellationToken.None, TimeSpan.FromSeconds(10));

            var leagueList = default(List<League>);
            try
            {
                leagueList = LeagueListCreator.CreateLeagueListFromJSONString(jsonString);
            }
            catch (Newtonsoft.Json.JsonSerializationException)
            {
                try
                {
                    var error = ErrorCreator.CreateErrorFromJSONString(jsonString);
                    if (error != null && error.error != null)
                    {
                        throw new PoEApiException("An error occurred whilst deserializing league list.", error);
                    }

                }
                catch (Newtonsoft.Json.JsonSerializationException) { }//who knows what type of json string it is.
            }
            return leagueList ?? new List<League>();
        }

    }
}
