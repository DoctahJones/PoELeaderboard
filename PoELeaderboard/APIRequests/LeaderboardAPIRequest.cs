using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace PoELeaderboard.APIRequests
{
    public class LeaderboardAPIRequest : APIRequest
    {
        public LeaderboardAPIRequest(IAPIRequestSender apiRequestSender) : base(apiRequestSender)
        {
        }

        public LeaderboardAPIRequest(IAPIRequestSender apiRequestSender, string league, int limit = 20, int offset = 0, string type = "league", int difficulty = 1)
            :base(apiRequestSender)
        {
            CreateRequest(league, limit, offset, type, difficulty);
        }


        public void CreateRequest(string league, int limit = 200, int offset = 0, string type = "league", int difficulty = 1)
        {
            if (limit > 200 || limit < 1)
            {
                throw new ArgumentOutOfRangeException("limit", "Value should be greater than 0 and has a mazimum of 200.");
            }
            if (offset < 0 || offset >= 15000)
            {
                throw new ArgumentOutOfRangeException("offset", "Value should be greater than 0 and has a mazimum of 15,000.");
            }
            if (difficulty < 1 || difficulty > 4)
            {
                throw new ArgumentOutOfRangeException("difficulty", "Value should be 1 (Standard), 2 (Cruel), 3 (Merciless) or 4 (Eternal)");
            }
            string request = "http://api.pathofexile.com/ladders/" + league + "?offset=" + offset + "&limit=" + limit + "&type=" + type + "&track=true";
            if (type == "labyrinth")
            {
                request += "&difficulty=" + difficulty;
            }
            Url = request;
        }

        







    }
}
