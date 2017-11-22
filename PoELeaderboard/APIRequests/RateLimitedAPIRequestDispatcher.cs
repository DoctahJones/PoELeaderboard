using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Threading;

namespace PoELeaderboard.APIRequests
{
    /// <summary>
    /// Class that sends web requests with a delay.
    /// </summary>
    public class RateLimitedAPIRequestDispatcher
    {
        /// <summary>
        /// Semaphore that prevents more than 1 task running at a time.
        /// </summary>
        private SemaphoreSlim semaphore = new SemaphoreSlim(1);

        /// <summary>
        /// Semaphore that prevents more than 1 priority task running at a time.
        /// </summary>
        private SemaphoreSlim prioritySemaphore = new SemaphoreSlim(1);

        /// <summary>
        /// Whether a priority request wants to run. Use priorityLock to access it.
        /// </summary>
        private bool pauseForPriorityRequested = false;
        private readonly object priorityLock = new Object();

        /// <summary>
        /// Auto reset event to pause normally running requests to run a priority request.
        /// </summary>
        AsyncAutoResetEvent priortyResetEvent = new AsyncAutoResetEvent(false);

        /// <summary>
        /// The delay between APIRequests.
        /// </summary>
        public TimeSpan DelayPeriod { get; set; }


        public RateLimitedAPIRequestDispatcher(TimeSpan delayPeriod)
        {
            DelayPeriod = delayPeriod;
        }


        public async Task<string> MakeAPIRequestAsync(APIRequest request, CancellationToken cancelToken, TimeSpan timeOut)
        {
            await semaphore.WaitAsync(cancelToken);
            bool localPauseState;
            lock (priorityLock)
            {
                localPauseState = pauseForPriorityRequested;
            }
            if (localPauseState)
            {
                await priortyResetEvent.WaitAsync();
            }
            try
            {
                var timer = Task.Delay(DelayPeriod);
                var task = request.SendRequestAsync(cancelToken, timeOut);
                var taskAndTimer = new Task[] { task, timer };
                await Task.WhenAll(taskAndTimer);
                return task.Result;
            }
            finally
            {
                semaphore.Release();
            }
        }

        
        public async Task<string> MakeAPIRequestAsyncPriority(APIRequest request, CancellationToken cancelToken, TimeSpan timeOut)
        {
            await prioritySemaphore.WaitAsync(cancelToken);
            lock (priorityLock)
            {
                pauseForPriorityRequested = true;
            }
            try
            {
                await Task.Delay(DelayPeriod);//Probably better way of making sure delay since last normal request has taken place than this.
                var timer = Task.Delay(DelayPeriod);
                var task = request.SendRequestAsync(cancelToken, timeOut);
                var taskAndTimer = new Task[] { task, timer };
                await Task.WhenAll(taskAndTimer);
                return task.Result;
            }
            finally
            {
                lock (priorityLock)
                {
                    pauseForPriorityRequested = false;
                }
                priortyResetEvent.Set();
                prioritySemaphore.Release();
            }
        }
    }
}
