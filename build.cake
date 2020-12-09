var target = Argument("target", "Test");
var configuration = Argument("configuration", "Release");

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("Build")    
    .Does(() =>
{
    DotNetCoreBuild("./IntegrationTestsWithDocker.sln", new DotNetCoreBuildSettings
    {
        Configuration = configuration,
    });       
});

Task("Test")
    .IsDependentOn("Build")
    .Does(() =>
{
    DotNetCoreTest("./IntegrationTestsWithDocker.sln", new DotNetCoreTestSettings
    {
        Configuration = configuration,
        NoBuild = true,
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

        // while(!IsDBContainerReady())
        // {
        //     Information("Database container not ready yet...");
        //     System.Threading.Thread.Sleep(10_000);
        // }

        // Information("DB container is ready!"); 
        System.Threading.Thread.Sleep(25_000);

        try
        {
            DotNetCoreTest("./tests/App.IntegrationTests/App.IntegrationTests.csproj", 
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
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);