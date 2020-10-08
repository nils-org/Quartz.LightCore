namespace Quartz.LightCore
{
    using System;
    using System.Globalization;
    using System.Text;

    using global::LightCore;

    // ReSharper disable once RedundantNameQualifier - collides with SA1135
    using Quartz.Spi;

    // inspired by https://knightcodes.com/.net/2016/08/15/dependency-injection-for-quartz-net.html

    /// <summary>
    /// The ResolverFactory for Quartz.Net. Used internally.
    /// Configure Quartz.Net to use this factory using the <see cref="SchedulerExtension"/>.
    /// </summary>
    /// <seealso cref="IJobFactory" />
    internal class LightCoreResolverJobFactory : IJobFactory
    {
        private readonly IContainer container;

        /// <summary>
        /// Initializes a new instance of the <see cref="LightCoreResolverJobFactory"/> class.
        /// </summary>
        /// <param name="container">The container.</param>
        internal LightCoreResolverJobFactory(IContainer container)
        {
            this.container = container;
        }

        /// <summary>
        /// Creates new job.
        /// </summary>
        /// <param name="bundle">The TriggerFiredBundle from which the <see cref="T:Quartz.IJobDetail" />
        /// and other info relating to the trigger firing can be obtained.</param>
        /// <param name="scheduler">a handle to the scheduler that is about to execute the job.</param>
        /// <returns>
        /// the newly instantiated Job.
        /// </returns>
        /// <exception cref="ArgumentNullException">If bundle is null.</exception>
        /// <exception cref="SchedulerException">
        /// Either, on failed resolutions (in this case the inner exception will be a <see cref="ResolutionFailedException"/>
        /// or on instantiation-exceptions (in this case the inner exception will be something different).
        /// </exception>
        /// <remarks>
        /// It should be extremely rare for this method to throw an exception -
        /// basically only the case where there is no way at all to resolve or instantiate
        /// and prepare the Job for execution.  When the exception is thrown, the
        /// Scheduler will move all triggers associated with the Job into the
        /// <see cref="F:Quartz.TriggerState.Error" /> state, which will require human
        /// intervention (e.g. an application restart after fixing whatever
        /// configuration problem led to the issue with instantiating the Job).
        /// </remarks>
        /// <throws>  SchedulerException if there is a problem instantiating the Job. </throws>
        public IJob NewJob(TriggerFiredBundle bundle, IScheduler scheduler)
        {
            if (bundle == null)
            {
                throw new ArgumentNullException(nameof(bundle));
            }

            try
            {
                return (IJob)container.Resolve(bundle.JobDetail.JobType);
            }
            catch (ResolutionFailedException resFailed)
            {
                var sb = new StringBuilder();
                sb.Append(string.Format(
                    CultureInfo.InvariantCulture,
                    "{0} Failed to construct type '{1}'. ",
                    nameof(LightCoreResolverJobFactory),
                    resFailed.ImplementationType.FullName));

                if (resFailed.InnerException == null)
                {
                    sb.AppendLine("Probably the type or a dependency was not registered.");
                }
                else
                {
                    sb.AppendLine(string.Format(
                        CultureInfo.InvariantCulture,
                        "The last error was: {0}: {1}",
                        resFailed.InnerException.GetType().Name,
                        resFailed.InnerException.Message));
                }

                throw new SchedulerException(sb.ToString(), resFailed);
            }
            catch (Exception e)
            {
                throw new SchedulerException(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "Problem while instantiating job '{0}' from the {1}.",
                        bundle.JobDetail.Key,
                        nameof(LightCoreResolverJobFactory)), e);
            }
        }

        /// <summary>
        /// Allows the job factory to destroy/cleanup the job if needed.
        /// </summary>
        /// <param name="job">The job.</param>
        public void ReturnJob(IJob job)
        {
            // see https://stackoverflow.com/questions/20587433/whats-the-purpose-of-returnjob-in-ijobfactory-interface-for-quartz-net
            // ReSharper disable once SuspiciousTypeConversion.Global
            (job as IDisposable)?.Dispose();
        }
    }
}
