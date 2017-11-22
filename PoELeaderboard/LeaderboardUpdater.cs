using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PoELeaderboard.APIRequests;
using PoELeaderboard.Models;
using System.Threading;
using System.Net;

namespace PoELeaderboard
{
    public class LeaderboardUpdater : ILeaderboardUpdater
    {
        /// <summary>
        /// The class that sends the api requests using a delay to prevent exceeding rate limit.
        /// </summary>
        private RateLimitedAPIRequestDispatcher requestDispatcher;

        private CancellationTokenSource cancellationTokenSource;


        private LeaderboardUpdateActionState currentUpdateState;
        private LeaderboardUpdateDetails currentDetails;



        public TimeSpan TimeOut { get; set; }

        //The delegate for the event which can be subscribed to to receive updates about progress of leaderboard updates.
        public delegate void LeaderboardUpdateProgressHandler(Object sender, LeaderboardUpdateProgressEventArgs e);
        //The event which can be subscribed to to be notified when leaderboard update prgression has occurred..
        public event LeaderboardUpdateProgressHandler LeaderboardUpdateProgress;


        public LeaderboardUpdater(RateLimitedAPIRequestDispatcher requestDispatcher)
        {
            this.requestDispatcher = requestDispatcher;
            TimeOut = TimeSpan.FromSeconds(10);
        }

        public async Task<Leaderboard> CreateLeaderboardFromRangeAsync(LeaderboardUpdateDetails details)
        {
            this.currentDetails = details;
            var leaderboardRequest = new LeaderboardAPIRequest(new APIRequestSenderJSON());
            leaderboardRequest.CreateRequest(details.League.Name, details.Limit, details.Offset, details.Type, details.Difficulty);

            cancellationTokenSource = new CancellationTokenSource();

            string leaderboardData = default(string);

            leaderboardData = await this.StartRequestAsync(leaderboardRequest);

            var leader = LeaderboardCreator.CreateLeaderboardFromJSONString(leaderboardData, details);
            return leader;
        }

        public async Task<Leaderboard> CreateLeaderboardFromRangeAsyncWithRetry(int retryCount, LeaderboardUpdateDetails details)
        {
            bool success = false;
            int retryRemaining = retryCount;
            Leaderboard leaderboard = default(Leaderboard);
            while (retryRemaining > 0 && success == false)
            {
                retryRemaining--;
                try
                {
                    leaderboard = await CreateLeaderboardFromRangeAsync(details);
                }
                catch (PoEApiException e)
                {
                    if (e.Error.error.code == 2) //invalid query, possibly due to leaderboard not having 200 items in it initally.
                    {
                        throw e;
                    }
                    if (retryRemaining == 0)
                    {
                        throw e;
                    }
                }
                success = true;
            }
            return leaderboard;
        }


        public async Task<Leaderboard> GetLeaderboardFullAsync(LeaderboardUpdateDetails details)
        {
            this.currentDetails = details;
            this.currentUpdateState = LeaderboardUpdateActionState.FETCHING_DETAILS;
            OnLeaderboardUpdateProgressed(1, 0);

            int firstRequestSize = 200;
            details.Limit = firstRequestSize;
            details.Offset = 0;
            Leaderboard leaderboard = default(Leaderboard);
            try
            {
                leaderboard = await CreateLeaderboardFromRangeAsyncWithRetry(4, details);
            }
            catch (PoEApiException e)
            {
                if (e.Error.error.code == 2) //invalid query, possibly due to leaderboard not having 200 items in it initally.
                {
                    details.Limit = firstRequestSize = 1;
                }
            }
            if (this.currentUpdateState == LeaderboardUpdateActionState.CANCELLED) { return new Leaderboard(); };

            leaderboard = await CreateLeaderboardFromRangeAsyncWithRetry(4, details);
            OnLeaderboardUpdateProgressed(1, 1);

            if (leaderboard == null)
            {
                Console.WriteLine("uh oh should not be null");
            }
            int leaderboardSize = leaderboard.CharacterCount;

            if (this.currentUpdateState == LeaderboardUpdateActionState.CANCELLED) { return new Leaderboard(); };
            this.currentUpdateState = LeaderboardUpdateActionState.UPDATING;
            var allTasks = GetFullLeaderboardTaskListBuilder(details, leaderboardSize, firstRequestSize);
            string[] result = null;
            try
            {
                result = await AsyncProgressReporter.WhenAllEx<string>(allTasks, LeaderboardUpdateProgressedAction, TimeSpan.FromSeconds(1));
            }
            finally
            {
                this.cancellationTokenSource.Dispose();
            }

            if (this.currentUpdateState == LeaderboardUpdateActionState.CANCELLED) { return new Leaderboard(); };
            var failedPositionsList = HandleTaskOutputAndReturnFailures(leaderboard, allTasks, result, (i) => firstRequestSize + (198 * i));

            if (failedPositionsList.Count == 0)
            {
                this.currentUpdateState = LeaderboardUpdateActionState.COMPLETED;
                OnLeaderboardUpdateProgressed(allTasks.Count, allTasks.Count);
                return leaderboard;
            }
            else
            {
                return await RetryFailedLeaderboardPositionsAsync(leaderboard, failedPositionsList, 2, details);
            }
        }

        private List<Task<string>> GetFullLeaderboardTaskListBuilder(LeaderboardUpdateDetails details, int leaderboardSize, int firstRequestSize)
        {
            cancellationTokenSource = new CancellationTokenSource();
            var allTasks = new List<Task<string>>();
            int currentFirstCharacter = firstRequestSize - 2 < 1 ? 1 : firstRequestSize - 2; //-2 over first request so we have some overlap.
            while (currentFirstCharacter < leaderboardSize)
            {
                var leaderboardRequest = new LeaderboardAPIRequest(new APIRequestSenderJSON());
                int charsLeftToFetch = leaderboardSize - currentFirstCharacter;
                leaderboardRequest.CreateRequest(details.League.Name, (charsLeftToFetch > 200 ? 200 : charsLeftToFetch), currentFirstCharacter, details.Type, details.Difficulty);

                currentFirstCharacter += 198;

                var currTask = StartRequestAsync(leaderboardRequest);
                allTasks.Add(currTask);
            }
            return allTasks;
        }


        private async Task<Leaderboard> RetryFailedLeaderboardPositionsAsync(Leaderboard leaderboard, List<int> failedPositions, int retryCount, LeaderboardUpdateDetails details)
        {
            this.currentUpdateState = LeaderboardUpdateActionState.RETRYING;

            var allTasks = GetFillLeaderboardPositionsTaskListBuilder(leaderboard, failedPositions, details);
            string[] result = null;
            try
            {
                result = await AsyncProgressReporter.WhenAllEx<string>(allTasks, LeaderboardUpdateProgressedAction, TimeSpan.FromSeconds(1));
            }
            finally
            {
                this.cancellationTokenSource.Dispose();
            }
            var failedPositionsList = HandleTaskOutputAndReturnFailures(leaderboard, allTasks, result, (i) => failedPositions.ElementAt(i));
            if (failedPositionsList.Count == 0 || retryCount == 0)
            {
                this.currentUpdateState = LeaderboardUpdateActionState.COMPLETED;
                OnLeaderboardUpdateProgressed(allTasks.Count, allTasks.Count);
                return leaderboard;
            }
            else
            {
                return await RetryFailedLeaderboardPositionsAsync(leaderboard, failedPositionsList, retryCount--, details);
            }
        }

        private List<Task<string>> GetFillLeaderboardPositionsTaskListBuilder(Leaderboard leaderboard, List<int> failedPositions, LeaderboardUpdateDetails details)
        {
            cancellationTokenSource = new CancellationTokenSource();
            var allTasks = new List<Task<string>>();
            foreach (int startPosition in failedPositions)
            {
                var leaderboardRequest = new LeaderboardAPIRequest(new APIRequestSenderJSON());
                int limit = startPosition + 200 > leaderboard.CharacterCount ? leaderboard.CharacterCount - startPosition : 200;
                leaderboardRequest.CreateRequest(leaderboard.LeaderboardLeague.Name, limit, startPosition, details.Type, details.Difficulty);

                var currTask = StartRequestAsync(leaderboardRequest);
                allTasks.Add(currTask);
            }
            return allTasks;
        }

        private async Task<string> StartRequestAsync(LeaderboardAPIRequest leaderboardRequest)
        {
            try
            {
                return await requestDispatcher.MakeAPIRequestAsync(leaderboardRequest, cancellationTokenSource.Token, TimeOut);
            }
            catch (WebException)
            {
                return string.Empty;
            }
            catch (TaskCanceledException)
            {
                return string.Empty;
            }
            catch (ObjectDisposedException)
            {
                return string.Empty;
            }
        }

        private List<int> HandleTaskOutputAndReturnFailures(Leaderboard leaderboard, List<Task<string>> allTasks, string[] result, Func<int, int> methodToGetStartPositionOfFailedTask)
        {
            if (leaderboard == null)
            {
                throw new ArgumentNullException("leaderboard");
            }
            if (result == null)
            {
                throw new ArgumentNullException("result");
            }
            var retries = new List<int>();
            for (int i = 0; i < allTasks.Count; i++)
            {
                var task = allTasks.ElementAt(i);
                if (task.Exception != null)
                {
                    Console.WriteLine("Exception in LeaderboardUpdater.HandleTaskOutputAndReturnFailures. Message : {0}", task.Exception.Message);
                    retries.Add(methodToGetStartPositionOfFailedTask(i));
                }
                else if (task.IsCanceled == true)
                {
                    if (cancellationTokenSource.IsCancellationRequested)
                    {
                        Console.WriteLine("Task cancelled in LeaderboardUpdater.HandleTaskOutputAndReturnFailures.");
                    }
                    else
                    {
                        Console.WriteLine("Task times out in LeaderboardUpdater.HandleTaskOutputAndReturnFailures.");
                        retries.Add(methodToGetStartPositionOfFailedTask(i));
                    }
                }
                else
                {
                    if (result[i] != string.Empty)
                    {
                        try
                        {
                            LeaderboardCreator.AppendToLeaderboardFromJSONString(result[i], leaderboard);
                            Console.WriteLine("{0} Characters.", leaderboard.LeaderboardCharacters.Count);
                        }
                        catch (PoEApiException e)
                        {
                            Console.WriteLine("Unable to append task {0} to Leaderboard. Error message was: {1}", i, e.Error.error.message);
                            retries.Add(methodToGetStartPositionOfFailedTask(i));
                        }
                    }
                }
            }
            return retries;
        }

        private void LeaderboardUpdateProgressedAction(ICollection<Task<string>> tasks)
        {
            if (this.currentDetails != null && this.currentUpdateState != LeaderboardUpdateActionState.CANCELLED)
            {
                OnLeaderboardUpdateProgressed(tasks.Count(), tasks.Count(task => task.IsCompleted));
            }
        }

        private void OnLeaderboardUpdateProgressed(int totalUpdates, int completedUpdates)
        {
            if (this.LeaderboardUpdateProgress != null)
            {
                var e = new LeaderboardUpdateProgressEventArgs(this.currentUpdateState, this.currentDetails.League, totalUpdates, completedUpdates);
                LeaderboardUpdateProgress(this, e);
            }
        }


        public void CancelRunningUpdates()
        {
            if (this.cancellationTokenSource != null)
            {
                try
                {
                    this.cancellationTokenSource.Cancel();
                    this.currentUpdateState = LeaderboardUpdateActionState.CANCELLED;
                    OnLeaderboardUpdateProgressed(0, 1);
                }
                catch (OperationCanceledException)
                { }
            }
        }


    }
}
