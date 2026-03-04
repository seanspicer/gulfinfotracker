var builder = DistributedApplication.CreateBuilder(args);

// Postgres: Docker container locally, Azure PostgreSQL Flexible Server when deployed via azd
var postgres = builder.AddAzurePostgresFlexibleServer("postgres")
    .RunAsContainer(p => p
        .WithDataVolume("gulf-info-tracker-pgdata")
        .WithLifetime(ContainerLifetime.Persistent));

var db    = postgres.AddDatabase("GulfInfoTracker");

// Redis: Docker container locally, Azure Cache for Redis when deployed via azd
var redis = builder.AddAzureRedis("redis").RunAsContainer();

var api = builder.AddProject<Projects.GulfInfoTracker_Api>("api")
                 .WithReference(db)
                 .WithReference(redis)
                 .WaitFor(db)
                 .WaitFor(redis)
                 .WithExternalHttpEndpoints();

// Key Vault: only wired in publish mode — local dev reads from appsettings.Development.json
if (builder.ExecutionContext.IsPublishMode)
{
    var kv = builder.AddAzureKeyVault("vault");
    api.WithReference(kv);
}

builder.AddJavaScriptApp("web", "../GulfInfoTracker.Web")
       .WithReference(api)
       .WaitFor(api)
       .WithHttpEndpoint(env: "PORT")
       .WithExternalHttpEndpoints();

builder.Build().Run();
