using LightCore;
using Quartz.Spi;
using System;
using System.Globalization;
using System.Text;

namespace Quartz.LightCore
{
    // inspired by https://knightcodes.com/.net/2016/08/15/dependency-injection-for-quartz-net.html
    internal class LightCoreResolverJobFactory : IJobFactory
    {
        private readonly IContainer container;

        public LightCoreResolverJobFactory(IContainer container)
        {
            this.container = container;
        }

        public IJob NewJob(TriggerFiredBundle bundle, IScheduler scheduler)
        {
            if (bundle == null)
            {
                throw new ArgumentNullException("bundle");
            }
            try
            {
                return (IJob)container.Resolve(bundle.JobDetail.JobType);
            }
            catch (ResolutionFailedException resFailed)
            {
                var sb = new StringBuilder();
                sb.Append(string.Format(CultureInfo.InvariantCulture,
                    "{0} Failed to construct type '{1}'. ", typeof(LightCoreResolverJobFactory).Name, resFailed.ImplementationType.FullName));

                if (resFailed.InnerException == null)
                {
                    sb.AppendLine("Probably the type or a dependency was not registered.");
                }
                else
                {
                    sb.AppendLine(string.Format(CultureInfo.InvariantCulture,
                        "The last error was: {0}: {1}", resFailed.InnerException.GetType().Name, resFailed.InnerException.Message));
                }
                throw new SchedulerException(sb.ToString(), resFailed);

            }
            catch (Exception e)
            {
                throw new SchedulerException(
                    string.Format(CultureInfo.InvariantCulture,
                    "Problem while instantiating job '{0}' from the {1}.",
                    bundle.JobDetail.Key, typeof(LightCoreResolverJobFactory).Name), e);
            }
        }

        public void ReturnJob(IJob job)
        {
            // see https://stackoverflow.com/questions/20587433/whats-the-purpose-of-returnjob-in-ijobfactory-interface-for-quartz-net
            (job as IDisposable)?.Dispose();
        }
    }
}
