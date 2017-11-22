using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PoELeaderboard.Models;
using System.Linq;
using System.Collections.Generic;

namespace PoELeaderboard
{
    [TestClass]
    public class LeaderboardCreatorTest
    {
        //example 2 from http://www.pathofexile.com/developer/docs/api-resource-ladders
        const string example2 = "{ \"total\": 15000, \"entries\": [ { \"rank\": 1, \"dead\": false, \"online\": false, \"character\": { \"name\": \"PenDora\", \"level\": 100, \"class\": \"Scion\", \"id\": \"cc248e0d23c849d71b40379d82dfc19b200bdb7b8ac63322f06de6483aaca5ea\", \"experience\": 4250334444 }, \"account\": { \"name\": \"Jin_melike\", \"challenges\": { \"total\": 0 } } } ] }";

        const string first10Standard = "{\"total\":15000,\"entries\":[{\"rank\":1,\"dead\":false,\"online\":false,\"character\":{\"name\":\"PenDora\",\"level\":100,\"class\":\"Scion\",\"id\":\"cc248e0d23c849d71b40379d82dfc19b200bdb7b8ac63322f06de6483aaca5ea\",\"experience\":4250334444},\"account\":{\"name\":\"Jin_melike\",\"challenges\":{\"total\":0}}},{\"rank\":2,\"dead\":false,\"online\":false,\"character\":{\"name\":\"TaylorSwiftVEVO\",\"level\":100,\"class\":\"Scion\",\"id\":\"b23f488245ffebc87616c9acf76fbbb3d534e0490a8b30a9be48f8fcc3941be0\",\"experience\":4250334444},\"account\":{\"name\":\"PeakingDuck\",\"challenges\":{\"total\":40},\"twitch\":{\"name\":\"peakingduck\"}}},{\"rank\":3,\"dead\":false,\"online\":false,\"character\":{\"name\":\"Fuck_One_Point_Two\",\"level\":100,\"class\":\"Berserker\",\"id\":\"46427575f1d5a3bcf44b533c790fb46a6f3d4ad1751cbd6046a105f85a38733a\",\"experience\":4250334444},\"account\":{\"name\":\"Hizbas\",\"challenges\":{\"total\":22},\"twitch\":{\"name\":\"hizbas\"}}},{\"rank\":4,\"dead\":false,\"online\":false,\"character\":{\"name\":\"Dear_Santa_UA\",\"level\":100,\"class\":\"Occultist\",\"id\":\"41240e2f9f7a56ebb409d6060619a14ffba154c61a84ef769bc9758361351ae8\",\"experience\":4250334444},\"account\":{\"name\":\"Valerchik\",\"challenges\":{\"total\":0}}},{\"rank\":5,\"dead\":false,\"online\":false,\"character\":{\"name\":\"VaalMulliSpark\",\"level\":100,\"class\":\"Scion\",\"id\":\"7580516c6997a4401ea0363a0cca9e83ae50e475fe437fedc51848f322c98b6a\",\"experience\":4250334444},\"account\":{\"name\":\"spinzter\",\"challenges\":{\"total\":36}}},{\"rank\":6,\"dead\":false,\"online\":false,\"character\":{\"name\":\"Sliqs\",\"level\":100,\"class\":\"Ranger\",\"id\":\"f73e30b470c74d3a4ebd3e3ee4e4410ac9164f95f7c1876d32e2c74a5f861a6c\",\"experience\":4250334444},\"account\":{\"name\":\"Redhill\",\"challenges\":{\"total\":24},\"twitch\":{\"name\":\"sliqs_\"}}},{\"rank\":7,\"dead\":false,\"online\":false,\"character\":{\"name\":\"xVisco\",\"level\":100,\"class\":\"Templar\",\"id\":\"b96614016b65e349212a77a9996edc34faa3c164529141064a2cef24ca132277\",\"experience\":4250334444},\"account\":{\"name\":\"xVisco\",\"challenges\":{\"total\":0}}},{\"rank\":8,\"dead\":false,\"online\":false,\"character\":{\"name\":\"xdukanx\",\"level\":100,\"class\":\"Chieftain\",\"id\":\"3fd75b1d62ed688b6fe6facf824a2bc95cf3546eeeb961ab45e3d32f8cb2bba8\",\"experience\":4250334444},\"account\":{\"name\":\"danieldukan\",\"challenges\":{\"total\":0}}},{\"rank\":9,\"dead\":false,\"online\":false,\"character\":{\"name\":\"Gigaways\",\"level\":100,\"class\":\"Ranger\",\"id\":\"c894d59151e36fdf72651efc5ea306dda3b64dbe4d686b05c73dfce8eeb7f9ba\",\"experience\":4250334444},\"account\":{\"name\":\"gigawayss\",\"challenges\":{\"total\":0}}},{\"rank\":10,\"dead\":false,\"online\":false,\"character\":{\"name\":\"SasusiaTesusia\",\"level\":100,\"class\":\"Pathfinder\",\"id\":\"c93359f2d95bbdbf14ad0adf3f54868fb08a98bba6a40dd0c7bf7c17216cecdb\",\"experience\":4250334444},\"account\":{\"name\":\"zeldzioszka\",\"challenges\":{\"total\":24},\"twitch\":{\"name\":\"lancmis2\"}}}]}";

        [TestMethod]
        public void CreateLeaderboardFromJSONStringEmptyString()
        {
            var l = LeaderboardCreator.CreateLeaderboardFromJSONString("", new LeaderboardUpdateDetails( new League {Name="League" }));
            Assert.AreEqual(default(List<Character>), l.LeaderboardCharacters);
        }

        [TestMethod]
        public void CreateLeaderboardFromJSONStringSucceedsExampleFromAPIAuthor()
        {
            var l = LeaderboardCreator.CreateLeaderboardFromJSONString(example2, new LeaderboardUpdateDetails( new League { Name = "League" }));
            Assert.AreEqual(1, l.LeaderboardCharacters.Count);
            var c = l.LeaderboardCharacters.First();
            Assert.AreEqual("PenDora", c.CharacterName);
            Assert.AreEqual("Jin_melike", c.AccountName);
            Assert.AreEqual(1 ,c.Rank);
            Assert.AreEqual(100 ,c.Level); 
            Assert.AreEqual("Scion", c.CharacterClass); 
            Assert.AreEqual(4250334444, c.Experience); 
            Assert.AreEqual(false, c.Online); 
            Assert.AreEqual(false, c.Dead); 
            Assert.AreEqual(0, c.Challenges); 
            Assert.AreEqual(0, c.TimeLab);
            Assert.AreEqual("cc248e0d23c849d71b40379d82dfc19b200bdb7b8ac63322f06de6483aaca5ea", c.Id); 
        }

        [TestMethod]
        public void CreateLeaderboardFromJSONStringSucceedsFirst10Standard()
        {
            var l = LeaderboardCreator.CreateLeaderboardFromJSONString(first10Standard, new LeaderboardUpdateDetails(new League { Name = "League" }));
            Assert.AreEqual(10, l.LeaderboardCharacters.Count);
        }

        [TestMethod]
        public void UpdateLeaderboardFromJSONStringSucceedsOverwriting()
        {
            var l = LeaderboardCreator.CreateLeaderboardFromJSONString(example2, new LeaderboardUpdateDetails( new League { Name = "League" }));
            //example2 is first item in first10Standard so should be overwritten meaning 10 and not 11 characters.
            LeaderboardCreator.AppendToLeaderboardFromJSONString(first10Standard, l);
            Assert.AreEqual(10, l.LeaderboardCharacters.Count);
        }
    }
}
