#tool "nuget:?package=NUnit.ConsoleRunner&version=3.10.0"
//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");

//////////////////////////////////////////////////////////////////////
// PREPARATION
//////////////////////////////////////////////////////////////////////

// Define directories.
var rootDir = Directory("..");
var srcDir = rootDir + Directory("src");
var binDir = rootDir + Directory("bin");
var distDir = rootDir + Directory("dist");

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("Clean")
    .Does(() =>
{
    CleanDirectory(binDir);
    CleanDirectory(distDir);
});


Task("Build")
    .Does(() =>
{
    foreach(var f in new[] {
        srcDir + File("Quartz.LightCore/Quartz.LightCore.sln")
    }) {
        NuGetRestore(f);
        MSBuild(f, settings => {
            settings.SetConfiguration(configuration);
            settings.Restore = true;
        });
    }
});

Task("Test")
    .IsDependentOn("Build")
    .Does(() => 
{
    var testAssemblies = GetFiles(srcDir + File("**/bin/**/*.Tests.dll"));
    Information($"running tests on {testAssemblies.Count} test-assemblies");
    NUnit3(testAssemblies, new NUnit3Settings 
    {
        Results = new[] { new NUnit3Result { FileName = (binDir + File("TestResult.xml")).ToString() } },
    });
});

Task("Dist")
    .IsDependentOn("Build")
    .Does(() => 
{
    var files = GetFiles(binDir.ToString() + "/**/*.nupkg");
    CopyFiles(files, distDir);
});

Task("PublishAppVeyor")
    .WithCriteria(() => AppVeyor.IsRunningOnAppVeyor)
    .IsDependentOn("Dist")
    .Does(() => 
{
    foreach(var file in GetFiles(distDir + File("**/*")))
    {
        AppVeyor.UploadArtifact(file);
    }
});


//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////

Task("Default")
    .IsDependentOn("Clean")
    .IsDependentOn("Build")
    .IsDependentOn("Test")
    .IsDependentOn("Dist");

Task("AppVeyor")
    .IsDependentOn("Default")
    .IsDependentOn("PublishAppVeyor");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);