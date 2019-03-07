Quartz.LightCore
================

[LightCore](https://github.com/JuergenGutsch/LightCore) bindings for [Quartz.Net](https://www.quartz-scheduler.net/)

Howto
-----
    
	// setup LightCore
    var builder = new ContainerBuilder();
    /* some fancy setup here */
    var container = builder.Build();

	// setup Quartz
	var scheduler = await new StdSchedulerFactory().GetScheduler().UseLightCoreResolverJobFacotry(container);
	scheduler.ScheduleJob(
        JobBuilder.Create<ThisJobWillBeInstanciatedUsingLightCore>().Build(),
        TriggerBuilder.Create().StartNow().Build());
	

