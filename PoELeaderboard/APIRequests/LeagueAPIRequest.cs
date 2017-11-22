using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoELeaderboard.APIRequests
{
    public class LeagueAPIRequest : APIRequest
    {
        public LeagueAPIRequest(IAPIRequestSender apiRequestSender) : base(apiRequestSender)
        {
            SetUrl();
        }

        public void SetUrl()
        {
            Url = "http://api.pathofexile.com/leagues?type=main";
        }
    }
}
