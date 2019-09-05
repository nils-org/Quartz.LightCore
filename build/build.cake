#tool "nuget:?package=NUnit.ConsoleRunner&version=3.10.0"
#tool "nuget:?package=Wyam&version=2.2.7"
#tool "nuget:?package=KuduSync.NET&version=1.5.2"
#addin "nuget:?package=Cake.Wyam&version=2.2.7"
#addin "nuget:?package=Cake.Kudu&version=0.10.1"
#addin "nuget:?package=Cake.Git&version=0.21.0"

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
var docDir = rootDir + Directory("docs");
var binDir = rootDir + Directory("bin");
var docBinDir = binDir + Directory("docs");
var distDir = rootDir + Directory("dist");

var wyamSettings = new WyamSettings()
{
    InputPaths = new[]{ MakeAbsolute(docDir) },
    OutputPath = MakeAbsolute(docBinDir),
    ConfigurationFile = rootDir + File("config.wyam")
};

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

Task("CleanDocumentation")
    .Does(()=>
{
    CleanDirectory(docBinDir);
});

Task("CreateDocumentation")
    .IsDependentOn("CleanDocumentation")
    .Does(()=>
{
    Wyam(wyamSettings);
});

Task("PublishDocumentation")
    .IsDependentOn("CreateDocumentation")
    .Does(()=>
{
    // mostly stolen form https://github.com/cake-contrib/Cake.Recipe/blob/develop/Cake.Recipe/Content/wyam.cake

    var currentBranch = GitBranchCurrent(rootDir);
    Information("Current branch is: " + currentBranch.FriendlyName);
    if(currentBranch.FriendlyName != "master")
    {
        Warning("Not building on master. Skipping publish of docs.");
        return;
    }

    // local use:
    // > $env:PUBDOC_REMOTE = "https://github.com/nils-a/Quartz.LightCore.git"
    // > $env:PUBDOC_ACCESSTOKEN = "pat-from-git"
    
    var remote = EnvironmentVariable("PUBDOC_REMOTE");
    var accesstoken = EnvironmentVariable("PUBDOC_ACCESSTOKEN");
    var branch = "gh-pages";

    if(string.IsNullOrEmpty(remote))
    {
        throw new Exception("no remote-repo found. Please set environment PUBDOC_REMOTE!");
    }
    if(string.IsNullOrEmpty(accesstoken))
    {
        throw new Exception("no accesstoken (PAT) found. Please set environment PUBDOC_ACCESSTOKEN!");
    }

    var checkoutTo = binDir + Directory($"docs-{DateTime.Now.ToString("docs-yyyyMMdd_HHmmss")}");

    Information($"cloning remote:{remote} / branch:{branch}");
    GitClone(remote, checkoutTo, new GitCloneSettings
    { 
        BranchName = branch
    });
    
    Information("Sync output files...");
    Kudu.Sync(docBinDir, checkoutTo, new KuduSyncSettings {
        ArgumentCustomization = args=>args.Append("--ignore").AppendQuoted(".git;CNAME")
    });

    if (!GitHasUncommitedChanges(checkoutTo))
    {
        Information("Docs unchanged. Nothing to do.");
        return;
    }

    Information("Staging all changes...");
    GitAddAll(checkoutTo);
    
    if(!GitHasStagedChanges(checkoutTo))
    {
        Information("Docs still unchanged. Nothing to do.");
        return;
    }

    Information("Commiting all changes...");
    var sourceCommit = GitLogTip(rootDir);
    GitCommit(
        checkoutTo,
        sourceCommit.Committer.Name,
        sourceCommit.Committer.Email,
        $"generated doc publish:\r\n  {sourceCommit.Sha}\r\n  {sourceCommit.Message}"
    );

    Information("Pushing all changes...");
    GitPush(checkoutTo, accesstoken, "x-oauth-basic", branch);

});

Task("preview")
    .Does(()=>
{
    var previewSettings = wyamSettings;
    previewSettings.Preview = true;
    Wyam(previewSettings);
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
    .IsDependentOn("PublishAppVeyor")
    .IsDependentOn("PublishDocumentation");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);