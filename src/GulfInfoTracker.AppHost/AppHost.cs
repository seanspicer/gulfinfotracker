var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres")
    .WithDataVolume("gulf-info-tracker-pgdata")
    .WithLifetime(ContainerLifetime.Persistent);

var db    = postgres.AddDatabase("GulfInfoTracker");
var redis = builder.AddAzureRedis("redis").RunAsContainer();

var api = builder.AddProject<Projects.GulfInfoTracker_Api>("api")
                 .WithReference(db)
                 .WithReference(redis)
                 .WaitFor(db)
                 .WaitFor(redis)
                 .WithExternalHttpEndpoints();

builder.AddJavaScriptApp("web", "../GulfInfoTracker.Web")
       .WithReference(api)
       .WaitFor(api)
       .WithHttpEndpoint(env: "PORT")
       .WithExternalHttpEndpoints();

builder.Build().Run();
