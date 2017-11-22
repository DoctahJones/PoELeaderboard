using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using PoELeaderboard.APIRequests;

namespace PoELeaderboard
{
    [TestClass]
    public class LeagueListCreatorTest
    {
        const string testString = "[{\"id\":\"Standard\",\"description\":\"The default game mode.\",\"registerAt\":null,\"event\":false,\"url\":\"http:\\/\\/pathofexile.com\\/forum\\/view-thread\\/71278\",\"startAt\":\"2013-01-23T21:00:00Z\",\"endAt\":null,\"rules\":[]},{\"id\":\"Hardcore\",\"description\":\"A character killed in the Hardcore league is moved to the Standard league.\",\"registerAt\":null,\"event\":false,\"url\":\"http:\\/\\/pathofexile.com\\/forum\\/view-thread\\/71276\",\"startAt\":\"2013-01-23T21:00:00Z\",\"endAt\":null,\"rules\":[{\"id\":4,\"name\":\"Hardcore\",\"description\":\"A character killed in Hardcore is moved to its parent league.\"}]},{\"id\":\"SSF Standard\",\"description\":\"SSF Standard\",\"registerAt\":null,\"event\":false,\"url\":\"http:\\/\\/pathofexile.com\\/forum\\/view-thread\\/1841357\",\"startAt\":\"2013-01-23T21:00:00Z\",\"endAt\":null,\"rules\":[{\"id\":24,\"name\":\"Solo\",\"description\":\"You may not party in this league.\"}]},{\"id\":\"SSF Hardcore\",\"description\":\"SSF Hardcore\",\"registerAt\":null,\"event\":false,\"url\":\"http:\\/\\/pathofexile.com\\/forum\\/view-thread\\/1841353\",\"startAt\":\"2013-01-23T21:00:00Z\",\"endAt\":null,\"rules\":[{\"id\":4,\"name\":\"Hardcore\",\"description\":\"A character killed in Hardcore is moved to its parent league.\"},{\"id\":24,\"name\":\"Solo\",\"description\":\"You may not party in this league.\"}]},{\"id\":\"Harbinger\",\"description\":\"Mysterious Harbingers roam Wraeclast and Oriath. They control and direct the creatures around them.\\n\\nThis is the default Path of Exile league.\",\"registerAt\":\"2017-08-04T19:30:00Z\",\"event\":false,\"url\":\"http:\\/\\/pathofexile.com\\/forum\\/view-thread\\/1931655\",\"startAt\":\"2017-08-04T20:00:00Z\",\"endAt\":\"2017-12-04T21:00:00Z\",\"rules\":[]},{\"id\":\"Hardcore Harbinger\",\"description\":\"Mysterious Harbingers roam Wraeclast and Oriath. They control and direct the creatures around them.\\n\\nA character killed in Hardcore Harbinger becomes a Standard character.\",\"registerAt\":\"2017-08-04T19:30:00Z\",\"event\":false,\"url\":\"http:\\/\\/pathofexile.com\\/forum\\/view-thread\\/1931654\",\"startAt\":\"2017-08-04T20:00:00Z\",\"endAt\":\"2017-12-04T21:00:00Z\",\"rules\":[{\"id\":4,\"name\":\"Hardcore\",\"description\":\"A character killed in Hardcore is moved to its parent league.\"}]},{\"id\":\"SSF Harbinger\",\"description\":\"SSF Harbinger\",\"registerAt\":\"2017-08-04T19:30:00Z\",\"event\":false,\"url\":\"http:\\/\\/pathofexile.com\\/forum\\/view-thread\\/1931653\",\"startAt\":\"2017-08-04T20:00:00Z\",\"endAt\":\"2017-12-04T21:00:00Z\",\"rules\":[{\"id\":24,\"name\":\"Solo\",\"description\":\"You may not party in this league.\"}]},{\"id\":\"SSF Harbinger HC\",\"description\":\"SSF HC Harbinger\",\"registerAt\":\"2017-08-04T19:30:00Z\",\"event\":false,\"url\":\"http:\\/\\/pathofexile.com\\/forum\\/view-thread\\/1931652\",\"startAt\":\"2017-08-04T20:00:00Z\",\"endAt\":\"2017-12-04T21:00:00Z\",\"rules\":[{\"id\":4,\"name\":\"Hardcore\",\"description\":\"A character killed in Hardcore is moved to its parent league.\"},{\"id\":24,\"name\":\"Solo\",\"description\":\"You may not party in this league.\"}]},{\"id\":\"10 Day Mayhem (ORE004)\",\"description\":\"Please check the Event Forums for more details.\",\"registerAt\":\"2017-11-24T19:30:00Z\",\"event\":true,\"url\":\"http:\\/\\/pathofexile.com\\/forum\\/view-thread\\/2026934\",\"startAt\":\"2017-11-24T20:00:00Z\",\"endAt\":\"2017-12-04T21:00:00Z\",\"rules\":[]},{\"id\":\"10 Day Mayhem HC (ORE005)\",\"description\":\"Please check the Event Forums for more details.\",\"registerAt\":\"2017-11-24T19:30:00Z\",\"event\":true,\"url\":\"http:\\/\\/pathofexile.com\\/forum\\/view-thread\\/2026937\",\"startAt\":\"2017-11-24T20:00:00Z\",\"endAt\":\"2017-12-04T21:00:00Z\",\"rules\":[{\"id\":4,\"name\":\"Hardcore\",\"description\":\"A character killed in Hardcore is moved to its parent league.\"}]},{\"id\":\"10 Day Mayhem HC SSF (ORE006)\",\"description\":\"Please check the Event Forums for more details.\",\"registerAt\":\"2017-11-24T19:30:00Z\",\"event\":true,\"url\":\"http:\\/\\/pathofexile.com\\/forum\\/view-thread\\/2026939\",\"startAt\":\"2017-11-24T20:00:00Z\",\"endAt\":\"2017-12-04T21:00:00Z\",\"rules\":[{\"id\":4,\"name\":\"Hardcore\",\"description\":\"A character killed in Hardcore is moved to its parent league.\"},{\"id\":24,\"name\":\"Solo\",\"description\":\"You may not party in this league.\"}]}]";



        [TestMethod]
        public void CreateLeagueListFromEmptyStringTest()
        {
            var emptyString = "";
            var t = LeagueListCreator.CreateLeagueListFromJSONString(emptyString);
            Assert.AreEqual(0, t.Count);
        }



        [TestMethod]
        public void CreateLeagueListFromTestStringTest()
        {
            var t = LeagueListCreator.CreateLeagueListFromJSONString(testString);
            Assert.AreEqual(11, t.Count);
        }
    }
}
