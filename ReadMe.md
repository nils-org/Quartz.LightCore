# Quartz.LightCore

[![Appveyor build][appveyorimage]][appveyor]
[![NuGet package][nugetimage]][nuget]

> [LightCore](https://github.com/JuergenGutsch/LightCore) bindings for [Quartz.Net](https://www.quartz-scheduler.net/)



## Howto

```
// setup LightCore
var builder = new ContainerBuilder();
/* some fancy setup here */
var container = builder.Build();

// setup Quartz
var scheduler = await new StdSchedulerFactory().GetScheduler().UseLightCoreResolverJobFacotry(container);
scheduler.ScheduleJob(
	JobBuilder.Create<ThisJobWillBeInstanciatedUsingLightCore>().Build(),
	TriggerBuilder.Create().StartNow().Build());
```

See [full documentation](https://nils-org.github.io/Quartz.LightCore/) for more

[appveyor]: https://ci.appveyor.com/project/nils-a/quartz-lightcore
[appveyorimage]: https://img.shields.io/appveyor/ci/nils-a/quartz-lightcore.svg?logo=appveyor&style=flat-square
[nuget]: https://nuget.org/packages/Quartz.LightCore
[nugetimage]: https://img.shields.io/nuget/v/Quartz.LightCore.svg?logo=nuget&style=flat-square
