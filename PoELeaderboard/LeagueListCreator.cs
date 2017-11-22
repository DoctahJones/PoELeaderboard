using PoELeaderboard.Models;
using System.Collections.Generic;

namespace PoELeaderboard
{
    public class LeagueJSON
    {
        public string id { get; set; }
        public string description { get; set; }
        public string registerAt { get; set; }
        public bool @event { get; set; }
        public string url { get; set; }
        public string startAt { get; set; }
        public string endAt { get; set; }
        public List<object> rules { get; set; }
    }

    public class LeagueListCreator
    {
        public static List<League> CreateLeagueListFromJSONString(string jsonString)
        {
            var list = new List<LeagueJSON>();
            list = Newtonsoft.Json.JsonConvert.DeserializeObject<List<LeagueJSON>>(jsonString);

            var returnList = new List<League>();
            if (list != null)
            {
                foreach (var l in list)
                {
                    returnList.Add(new League { Name = l.id, StartDate = l.startAt, EndDate = l.endAt });
                } 
            }
            return returnList;
        }
    }


}
