using LightCore;
using Quartz.Spi;
using System;
using System.Globalization;

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
            catch (Exception e)
            {
                throw new SchedulerException(
                    string.Format(CultureInfo.InvariantCulture, "Problem while instantiating job '{0}' from the NinjectJobFactory.", bundle.JobDetail.Key), e);
            }
        }

        public void ReturnJob(IJob job)
        {
            // see https://stackoverflow.com/questions/20587433/whats-the-purpose-of-returnjob-in-ijobfactory-interface-for-quartz-net
            (job as IDisposable)?.Dispose();
        }
    }
}
