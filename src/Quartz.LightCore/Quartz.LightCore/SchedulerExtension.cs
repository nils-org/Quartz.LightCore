namespace Quartz.LightCore
{
    using System.Threading.Tasks;
    using global::LightCore;

    /// <summary>
    /// <para>
    /// Extensions on <see cref="IScheduler" /> to enable using a LightCore <see cref="IContainer" />.
    /// </para>
    /// <para>
    /// Example:
    /// <code>
    /// <![CDATA[// setup LightCore
    /// var builder = new ContainerBuilder();
    /// /* some fancy setup here */
    /// var container = builder.Build();
    ///
    /// // setup Quartz
    /// var scheduler = await new StdSchedulerFactory().GetScheduler().UseLightCoreResolverJobFacotry(container);
    /// scheduler.ScheduleJob(
    ///     JobBuilder.Create<ThisJobWillBeInstanciatedUsingLightCore>().Build(),
    /// TriggerBuilder.Create().StartNow().Build());]]>
    /// </code>
    /// </para>
    /// </summary>
    public static class SchedulerExtension
    {
        /// <summary>
        /// Use LightCore as the JobFactory when not working async.
        /// </summary>
        /// <param name="scheduler">The Quartz <see cref="IScheduler"/> to configure.</param>
        /// <param name="container">The LightCore <see cref="IContainer"/> to use.</param>
        /// <returns>The scheduler for fluent usage.</returns>
        public static IScheduler UseLightCoreResolverJobFacotry(this IScheduler scheduler, IContainer container)
        {
            scheduler.JobFactory = new LightCoreResolverJobFactory(container);
            return scheduler;
        }

        /// <summary>
        /// Use LightCore as the JobFactory when not working async.
        /// </summary>
        /// <param name="schedulerTask">The Quartz <see cref="IScheduler"/>-Task to configure async.</param>
        /// <param name="container">The LightCore <see cref="IContainer"/> to use.</param>
        /// <returns>The scheduler(-task) for fluent usage.</returns>
        public static async Task<IScheduler> UseLightCoreResolverJobFacotry(this Task<IScheduler> schedulerTask, IContainer container)
        {
            var scheduler = await schedulerTask;
            return scheduler.UseLightCoreResolverJobFacotry(container);
        }
    }
}