using PoELeaderboard.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PoELeaderboard
{
    public class LeaderboardCreator
    {
        public static Leaderboard CreateLeaderboardFromJSONString(string jsonString, LeaderboardUpdateDetails details)
        {
            var leaderboard = new Leaderboard { LeaderboardLeague = details.League, Type = details.Type, Difficulty = Constants.GetDifficulties().Where(d => d.IntValue == details.Difficulty).First() };

            var leaderboardJSON = DeserializeToJSONLeaderboardStore(jsonString);

            if (leaderboardJSON != null)
            {
                List<Character> characters = CreateCharacterList(jsonString, leaderboardJSON);

                leaderboard.LeaderboardCharacters = characters;

                leaderboard.CharacterCount = leaderboardJSON.total;
            }
            return leaderboard;
        }

        private static List<Character> CreateCharacterList(string jsonString, JSONLeaderboardStore leaderboardJSON)
        {
            List<Character> characters = null;
            try
            {
                characters = CreateCharacterListFromJSON(leaderboardJSON);
            }
            catch (Newtonsoft.Json.JsonSerializationException)
            {
                var error = Newtonsoft.Json.JsonConvert.DeserializeObject<ErrorObject>(jsonString);
                if (error != null && error.error.message != null)
                {
                    throw new PoEApiException("Unable to deserialize string.", error);
                }
            }
            return characters;
        }

        public static Leaderboard AppendToLeaderboardFromJSONString(string jsonString, Leaderboard leaderboard)
        {
            var leaderboardJSON = DeserializeToJSONLeaderboardStore(jsonString);
            if (leaderboardJSON != null)
            {
                List<Character> newCharacters = CreateCharacterList(jsonString, leaderboardJSON);
                if (newCharacters != null)
                {
                    var leaderboardDictionary = leaderboard.LeaderboardCharacters.ToDictionary(c => c.Id);

                    foreach (Character c in newCharacters)
                    {
                        Character foundChar;
                        if (leaderboardDictionary.TryGetValue(c.Id, out foundChar))
                        {
                            leaderboard.LeaderboardCharacters.Remove(foundChar);
                        }
                        leaderboard.LeaderboardCharacters.Add(c);
                    }
                }
            }
            return leaderboard;
        }

        private static JSONLeaderboardStore DeserializeToJSONLeaderboardStore(string jsonString)
        {
            var leaderboardJSON = new JSONLeaderboardStore();
            leaderboardJSON = Newtonsoft.Json.JsonConvert.DeserializeObject<JSONLeaderboardStore>(jsonString);
            if (leaderboardJSON == null)
            {
                var error = Newtonsoft.Json.JsonConvert.DeserializeObject<ErrorObject>(jsonString);
                if (error != null && error.error.message != null)
                {
                    throw new PoEApiException("Unable to deserialize string.", error);
                }
            }
            return leaderboardJSON;
        }

        private static List<Character> CreateCharacterListFromJSON(JSONLeaderboardStore leaderboardJSON)
        {
            if (leaderboardJSON == null || leaderboardJSON.entries == null)
            {
                throw new Newtonsoft.Json.JsonSerializationException("Trying to create a character list from a null object.");
            }
            var returnList = new List<Character>();
            foreach (Entry c in leaderboardJSON.entries)
            {
                returnList.Add(new Character
                {
                    CharacterName = c.character.name,
                    AccountName = c.account.name,
                    Rank = c.rank,
                    Level = c.character.level == null ? 0 : (long)c.character.level,
                    CharacterClass = c.character.@class,
                    Experience = c.character.experience == null ? 0 : (long)c.character.experience,
                    Online = c.online,
                    Dead = c.dead,
                    Challenges = c.account.challenges.total,
                    TimeLab = c.time,
                    Id = c.character.id
                });
            }
            return returnList;
        }
    }
    public class CharacterJSON
    {
        public string name { get; set; }
        public object level { get; set; }
        public string @class { get; set; }
        public string id { get; set; }
        public object experience { get; set; }
    }

    public class Challenges
    {
        public int total { get; set; }
    }

    public class Twitch
    {
        public string name { get; set; }
    }

    public class Account
    {
        public string name { get; set; }
        public Challenges challenges { get; set; }
        public Twitch twitch { get; set; }
    }

    public class Entry
    {
        public int rank { get; set; }
        public bool dead { get; set; }
        public bool online { get; set; }
        public int time { get; set; }
        public CharacterJSON character { get; set; }
        public Account account { get; set; }
    }

    public class JSONLeaderboardStore
    {
        public int total { get; set; }
        public string title { get; set; }
        public int startTime { get; set; }
        public List<Entry> entries { get; set; }
    }

}