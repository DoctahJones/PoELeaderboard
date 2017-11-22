using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoELeaderboard
{
    public static class AsyncProgressReporter
    {
        /// <summary>
        /// Takes a collection of tasks and completes the returned task when all tasks have completed. If completion
        /// takes a while a progress lambda is called where all tasks can be observed for their status.
        /// From:
        /// https://stackoverflow.com/questions/36149241/showing-progress-while-waiting-for-all-tasks-in-listtask-to-complete/36154418#36154418
        /// </summary>
        /// <param name="tasks"></param>
        /// <param name="reportProgressAction"></param>
        /// <returns></returns>
        public static async Task WhenAllEx(ICollection<Task> tasks, Action<ICollection<Task>> reportProgressAction, TimeSpan progressInterval)
        {
            // get Task which completes when all 'tasks' have completed
            var whenAllTask = Task.WhenAll(tasks);
            for (; ; )
            {
                var timer = Task.Delay(progressInterval);
                // Wait until either all tasks have completed OR 250ms passed
                await Task.WhenAny(whenAllTask, timer);
                // if all tasks have completed, complete the returned task
                if (whenAllTask.IsCompleted)
                {
                    return;
                }
                // Otherwise call progress report lambda and do another round
                reportProgressAction(tasks);
            }
        }

        public static async Task<T[]> WhenAllEx<T>(ICollection<Task<T>> tasks, Action<ICollection<Task<T>>> reportProgressAction, TimeSpan progressInterval)
        {
            // get Task which completes when all 'tasks' have completed
            var whenAllTask = Task.WhenAll(tasks);
            for (; ; )
            {
                var timer = Task.Delay(progressInterval);
                // Wait until either all tasks have completed OR 250ms passed
                await Task.WhenAny(whenAllTask, timer);
                // if all tasks have completed, complete the returned task
                if (whenAllTask.IsCompleted)
                {
                    return await whenAllTask;
                }
                reportProgressAction(tasks);
            }
        }

        
    }
}
