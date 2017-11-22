using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using PoELeaderboard.Models;
using System.Linq;
using System.Collections.Generic;

namespace PoELeaderboard
{
    [TestClass]
    public class LeaderboardCollectionTest
    {
        class MockUpdater : ILeaderboardUpdater
        {
            public event LeaderboardUpdater.LeaderboardUpdateProgressHandler LeaderboardUpdateProgress;

            int count = 0;

            public TimeSpan TimeOut { get; set; }

            public async Task<Leaderboard> CreateLeaderboardFromRangeAsync(LeaderboardUpdateDetails details)
            {
                return await CreateLeaderboardAsync(details);
            }

            public async Task<Leaderboard> CreateLeaderboardFromRangeAsyncWithRetry(int retryCount, LeaderboardUpdateDetails details)
            {
                return await CreateLeaderboardAsync(details);
            }

            public async Task<Leaderboard> GetLeaderboardFullAsync(LeaderboardUpdateDetails details)
            {
                return await CreateLeaderboardAsync(details);
            }

            public void CancelRunningUpdates()
            {
                throw new NotImplementedException();
            }

            private async Task<Leaderboard> CreateLeaderboardAsync(LeaderboardUpdateDetails details)
            {
                Leaderboard r = await Task.Run(() => CreateLeaderboard(details));
                return r;
            }

            private Leaderboard CreateLeaderboard(LeaderboardUpdateDetails details)
            {
                var l = new Leaderboard();
                l.Difficulty = Constants.GetDifficulties().Where(d => d.IntValue == details.Difficulty).First();
                l.LeaderboardLeague = details.League;
                l.Type = details.Type;
                l.CharacterCount = 10 + count++;
                var chars = new List<Character>();
                for (int i = 0; i < 10; i++)
                {
                    chars.Add(new Character { CharacterName = "billy" + i, Rank = i + 1, Level = i + 15 });
                }
                l.LeaderboardCharacters = chars;
                return l;
            }
        }

        [TestMethod]
        public async Task GetLeaderboardReturnsCorrectLeagueTest()
        {
            var lc = new LeaderboardCollection(new MockUpdater());
            var league = new League { Name = "Standard" };
            await lc.GetFullLeaderboardFromWeb(league, "league", 1);

            var result = lc.GetLeaderboard(league, "league", 1);

            Assert.IsNotNull(result);
            Assert.AreEqual(league, result.LeaderboardLeague);
        }

        [TestMethod]
        public void GetLeaderboardReturnsNothingWhenLeagueDoesntExistTest()
        {
            var lc = new LeaderboardCollection(new MockUpdater());
            var league = new League { Name = "Standard" };

            var result = lc.GetLeaderboard(league, "league", 1);

            Assert.AreEqual(default(Leaderboard), result);
        }

        [TestMethod]
        public async Task GetLeaderboardReturnsCorrectDifficultyForLabyrinthTest()
        {
            var lc = new LeaderboardCollection(new MockUpdater());
            var league = new League { Name = "Standard" };
            await lc.GetFullLeaderboardFromWeb(league, "labyrinth", 2);

            var result = lc.GetLeaderboard(league, "labyrinth", 2);

            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Difficulty.IntValue);
        }

        [TestMethod]
        public async Task GetLeaderboardReturnsNothingForOtherDifficultyLabyrinthTest()
        {
            var lc = new LeaderboardCollection(new MockUpdater());
            var league = new League { Name = "Standard" };
            await lc.GetFullLeaderboardFromWeb(league, "labyrinth", 2);

            var result = lc.GetLeaderboard(league, "labyrinth", 1);

            Assert.AreEqual(default(Leaderboard), result);
        }

        [TestMethod]
        public async Task GetLeaderboardReturnsUpdatedLeaderboardWhenOverwrittenTest()
        {
            var lc = new LeaderboardCollection(new MockUpdater());
            var league = new League { Name = "Standard" };
            await lc.GetFullLeaderboardFromWeb(league, "league", 1);

            var result = lc.GetLeaderboard(league, "league", 1);

            var count1 = result.CharacterCount;

            await lc.GetFullLeaderboardFromWeb(league, "league", 1);
            result = lc.GetLeaderboard(league, "league", 1);

            Assert.AreNotEqual(count1, result.CharacterCount);
        }

        [TestMethod]
        public async Task FetchingNewLeaderboardSendsEventTest()
        {
            var lc = new LeaderboardCollection(new MockUpdater());
            LeaderboardUpdateEventArgs args = null;
            lc.LeaderboardUpdate += (send, e) =>
            {
                args = e;
            };
            var league = new League { Name = "Standard" };
            await lc.GetFullLeaderboardFromWeb(league, "league", 1);

            Assert.IsNotNull(args);
        }

        [TestMethod]
        public async Task FetchingNewLeaderboardSendsCorrectLeagueTest()
        {
            var lc = new LeaderboardCollection(new MockUpdater());
            LeaderboardUpdateEventArgs args = null;
            lc.LeaderboardUpdate += (send, e) =>
            {
                args = e;
            };
            var league = new League { Name = "Standard" };
            await lc.GetFullLeaderboardFromWeb(league, "league", 1);

            Assert.IsNotNull(args);
            Assert.AreEqual(league, args.LeagueUpdated);
        }
    }
}
