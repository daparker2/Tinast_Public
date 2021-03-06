﻿namespace DP.Tinast
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Task extension methods for Tinast.
    /// </summary>
    public static class TaskExtensionMethods
    {
        /// <summary>
        /// Timeouts the task after a given interval.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="task">The task.</param>
        /// <param name="timeout">The timeout.</param>
        /// <returns></returns>
        public static async Task<TResult> TimeoutAfter<TResult>(this Task<TResult> task, TimeSpan timeout)
        {
            await TimeoutAfter((Task)task, timeout).ConfigureAwait(false);
            return task.Result;
        }

        /// <summary>
        /// Timeouts the task after a given interval.
        /// </summary>
        /// <param name="task">The task.</param>
        /// <param name="timeout">The timeout.</param>
        /// <returns></returns>
        /// <exception cref="System.TimeoutException">The operation has timed out.</exception>
        [DebuggerNonUserCode]
        public static async Task TimeoutAfter(this Task task, TimeSpan timeout)
        {
            using (var timeoutCancellationTokenSource = new CancellationTokenSource())
            {
                Task delayTask = Task.Delay(timeout, timeoutCancellationTokenSource.Token);
                var completedTask = await Task.WhenAny(task, delayTask).ConfigureAwait(false);
                if (completedTask == task)
                {
                    timeoutCancellationTokenSource.Cancel();
                    try
                    {
                        await delayTask.ConfigureAwait(false);
                    }
                    catch (OperationCanceledException)
                    {
                    }

                    await task.ConfigureAwait(false);  // Very important in order to propagate exceptions
                }
                else
                {
                    throw new TimeoutException("The operation has timed out.");
                }
            }
        }
    }
}
