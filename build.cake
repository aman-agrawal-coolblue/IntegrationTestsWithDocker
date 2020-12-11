#addin "Cake.BuildSystems.Module&version=0.3.2"

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");
var version = Argument<string>("version", "0.0.1");

var name = "IntegrationTestsWithDocker";
var webServiceProjectDir = Directory("./src");
var webServicePublishDir = Directory(Directory(artifactsDir) + Directory(name));

const string defaultPackagedArtifactsDir = "./packaged";
const string acceptanceTestsProjGlob = "test/**/*.Test.Acceptance.csproj";
const string artifactsDir = "./artifacts";

var packagedArtifactsDir = Directory(Argument<string>("artifactPath", defaultPackagedArtifactsDir));
var environment = EnvironmentVariable("RELEASE_ENVIRONMENT"); // Used by "release" - Which is the target environment we are releasing to?

///////////////////////////////////////////////////////////////////////////////
// SETUP / TEARDOWN
///////////////////////////////////////////////////////////////////////////////

Setup(ctx =>
{
    var buildVersion = EnvironmentVariable("BUILD_NUMBER");
    if(string.IsNullOrWhiteSpace(buildVersion) == false)
    {
        Information("Version found from BUILD_NUMBER environment variable. Using new value of '{0}' instead of previously specified '{1}'.", buildVersion, version);
        version = buildVersion;
    }

    // Executed BEFORE the first task.
    Information("Running tasks...");
});


///////////////////////////////////////////////////////////////////////////////
// FUNCTIONS
///////////////////////////////////////////////////////////////////////////////

bool IsRunningOnTeamCity() => Context.BuildSystem().IsRunningOnTeamCity;

void Test(string projectGlob)
{
    var projects = GetFiles(projectGlob);
    if(!projects.Any())
        throw new Exception($"No tests found matching '{projectGlob}'.");

    var settings = new DotNetCoreTestSettings
                   {
                       Configuration = configuration,
                   };

    foreach (var project in projects)
    {
        DotNetCoreTest(project.FullPath, settings);
    }
}


bool IsDBContainerReady()
{
    using(var process = StartAndReturnProcess("docker", 
        new ProcessSettings
        {
            Arguments="ps",
            RedirectStandardOutput = true
        }))
    {
        process.WaitForExit();
        var exitCode = process.GetExitCode();

        if (exitCode == 0)
        {
            var outputs = process.GetStandardOutput();
            //Information(string.Join("",outputs));

            foreach (var item in outputs)
                if (item.Contains("mysql:latest"))
                    return true;
        }
        else
            throw new InvalidOperationException($"Process exited with {exitCode}");

        return false;
    }
}

void CleanUp()
{
    DockerComposeDown();
    RemoveContainerImage();    
}

void DockerComposeDown()
{
    using(var dcDown = StartAndReturnProcess("docker-compose",  
        new ProcessSettings
        {
            Arguments="-f docker-compose-db.yaml down"
        }))
    {
        dcDown.WaitForExit();
        Information(dcDown.GetExitCode());
    }
}

void RemoveContainerImage()
{
    using(var rm = StartAndReturnProcess("docker", 
        new ProcessSettings
        {
            Arguments="image rm mysql:latest -f"
        }))
    {
        rm.WaitForExit();
        Information(rm.GetExitCode());
    }
}

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("pullrequest")
    .Description("Runs on TeamCity after creating a PR.")
    .IsDependentOn("Build")
    .IsDependentOn("IntegrationTest");

Task("buildrelease-codedeploy")
    .Description("Create a new release and provide package ready for deployment")
    .IsDependentOn("Build")
    .IsDependentOn("Publish")
    .IsDependentOn("Package")
    .Does(() => {
        TeamCity.SetParameter("env.RELEASE_VERSION", version);
        TeamCity.PublishArtifacts(packagedArtifactsDir.Path.MakeAbsolute(Context.Environment).FullPath);
    });

Task("validatedeployment")
    .Description("Validates that the applications have correctly been deployed to their respective environments")
    .IsDependentOn("Test-Acceptance");

///////////////////////////////////////////////////////////////////////////////
// BUILD
///////////////////////////////////////////////////////////////////////////////

Task("Clean")
    .Description("Cleans all directories that are used during the build process.")
    .Does(() =>
{
    CleanDirectories("**/bin/" + configuration);
    CleanDirectories("**/obj/");
    CleanDirectory(artifactsDir);
});

Task("Build")    
    .IsDependentOn("Clean")
    .Does(() =>
{
    DotNetCoreBuild(name+".sln", new DotNetCoreBuildSettings
    {
        Configuration = configuration,
    });       
});

Task("IntegrationTest")
    .IsDependentOn("Build")
    .Does(()=>
{
    using(var process = StartAndReturnProcess("docker-compose", 
        new ProcessSettings
        {
            Arguments="-f docker-compose-db.yaml up -d --remove-orphans"
        }))
    {
        process.WaitForExit();
        Information($"Process exited with: {process.GetExitCode()}");        

        try
        {
            DotNetCoreTest(name + ".sln", 
                new DotNetCoreTestSettings
                {
                    Configuration = configuration,
                    NoBuild = true,
                });
        }
        finally
        {
            CleanUp();
        }
    }
});

Task("Test-Acceptance")
    .Does(() =>
{
    if (IsRunningOnTeamCity()
     && !string.Equals(environment, "acceptance", StringComparison.OrdinalIgnoreCase))
    {
        const string skipAcceptanceTestsMessage = "Skipping acceptance tests for non-acceptance environment.";
        Information(skipAcceptanceTestsMessage);
        TeamCity.WriteStatus(skipAcceptanceTestsMessage);
        return;
    }

    Test(acceptanceTestsProjGlob);
});


///////////////////////////////////////////////////////////////////////////////
// PACK
///////////////////////////////////////////////////////////////////////////////

Task("Publish")
    .Does(() =>
{
    PublishService(webServicePublishDir, webServiceProjectDir);
});

void PublishService(ConvertableDirectoryPath publishDir, ConvertableDirectoryPath projectDir)
{
    CreateDirectory(publishDir);
    CleanDirectory(publishDir);

    var settings = new DotNetCorePublishSettings
    {
        Configuration = configuration,
        OutputDirectory = publishDir.Path.FullPath,
        SelfContained = false,
        Runtime = "linux-x64",
    };

    DotNetCorePublish(projectDir.Path.FullPath, settings);
}

Task("Package")
    .Does(() =>
    {
        CreateDirectory(packagedArtifactsDir);
        Zip(Directory(artifactsDir) + Directory(name), packagedArtifactsDir + File(name + ".zip"));
    });

///////////////////////////////////////////////////////////////////////////////
// TARGETS
///////////////////////////////////////////////////////////////////////////////

Task("Default")
    .Description("This is the default task which will be ran if no specific target is passed in.")
    .IsDependentOn("pullrequest");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);