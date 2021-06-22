namespace Quartz.LightCore.Tests
{
    using Shouldly;

    using global::LightCore;
    
    // ReSharper disable once RedundantNameQualifier - collides with SA1135
    using Quartz.Impl;

    using System;
    using System.Threading;
    using System.Threading.Tasks;

    using Xunit;

    public class UnitTest1
    {
        [Fact]
        public async Task TestMethod1()
        {
            // Arrange
            var diWasCalled = false;
            var jobWasRun = false;
            var builder = new ContainerBuilder();
            builder.Register(x =>
            {
                diWasCalled = true;
                return new FakeJob(() => jobWasRun = true);
            });
            var container = builder.Build();

            var scheduler = await new StdSchedulerFactory().GetScheduler().UseLightCoreResolverJobFacotry(container);
            await scheduler.ScheduleJob(
                JobBuilder.Create<FakeJob>().Build(),
                TriggerBuilder.Create().StartNow().Build());

            // Act
            await scheduler.Start();

            // Assert
            while (!jobWasRun)
            {
                Thread.Sleep(10);
            }
            diWasCalled.ShouldBeTrue();
        }

        [Fact]
        public void SyncTestMethod1()

        {
            // Arrange
            var diWasCalled = false;
            var jobWasRun = false;
            var builder = new ContainerBuilder();
            builder.Register(x =>
            {
                diWasCalled = true;
                return new FakeJob(() => jobWasRun = true);
            });
            var container = builder.Build();

            var scheduler = new StdSchedulerFactory().GetScheduler().GetAwaiter().GetResult().UseLightCoreResolverJobFacotry(container);
            scheduler.ScheduleJob(
                JobBuilder.Create<FakeJob>().Build(),
                TriggerBuilder.Create().StartNow().Build()).GetAwaiter().GetResult();

            // Act
            scheduler.Start().GetAwaiter().GetResult();

            // Assert
            while (!jobWasRun)
            {
                Thread.Sleep(10);
            }
            diWasCalled.ShouldBeTrue();
        }

        private class FakeJob : IJob
        {
            private readonly Action callback;

            internal FakeJob(Action callback)
            {
                this.callback = callback;
            }

            public Task Execute(IJobExecutionContext context)
            {
                return new TaskFactory().StartNew(() =>
                {
                    callback();
                });
            }
        }
    }
}
