using System;
using System.Threading;
using System.Threading.Tasks;
using Foundatio.Utility;
using Foundatio.Logging;
using Foundatio.Queues;
using Microsoft.Extensions.Logging;

namespace Foundatio.Jobs {
    public interface IQueueJob<T> : IJob where T : class {
        /// <summary>
        /// Processes a queue entry and returns the result. This method is typically called from RunAsync() 
        /// but can also be called from a function passing in the queue entry.
        /// </summary>
        Task<JobResult> ProcessAsync(IQueueEntry<T> queueEntry, CancellationToken cancellationToken);
        IQueue<T> Queue { get; }
    }

    public static class QueueJobExtensions {
        public static void RunUntilEmpty<T>(this IQueueJob<T> job, CancellationToken cancellationToken = default(CancellationToken)) where T : class {
            var logger = job.GetLogger();
            job.RunContinuous(cancellationToken: cancellationToken, continuationCallback: async () => {
                var stats = await job.Queue.GetQueueStatsAsync().AnyContext();
                logger.LogTrace("RunUntilEmpty continuation: queue: {Queued} working={Working}", stats.Queued, stats.Working);
                return stats.Queued + stats.Working > 0;
            });
        }
    }
}