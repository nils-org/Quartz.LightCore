using LightCore;
using System.Threading.Tasks;

namespace Quartz.LightCore
{
    /// <summary>
    /// Extensions on <see cref="IScheduler" /> to enable using a LightCore <see cref="IContainer" /> />
    /// </summary>
    public static class SchedulerExtension
    {
        /// <summary>
        /// Use LightCore as the JobFactory
        /// </summary>
        /// <param name="scheduler"></param>
        /// <param name="container"></param>
        /// <returns></returns>
        public static IScheduler UseLightCoreResolverJobFacotry(this IScheduler scheduler, IContainer container)
        {
            scheduler.JobFactory = new LightCoreResolverJobFactory(container);
            return scheduler;
        }

        /// <summary>
        /// Use LightCore as the JobFactory
        /// </summary>
        /// <param name="schedulerTask"></param>
        /// <param name="container"></param>
        /// <returns></returns>
        public static async Task<IScheduler> UseLightCoreResolverJobFacotry(this Task<IScheduler> schedulerTask, IContainer container)
        {
            var scheduler = await schedulerTask;
            return scheduler.UseLightCoreResolverJobFacotry(container);
        }
    }
}
