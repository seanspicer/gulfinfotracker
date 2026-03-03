var builder = DistributedApplication.CreateBuilder(args);

var sql   = builder.AddAzureSqlServer("sql").RunAsContainer();
var db    = sql.AddDatabase("GulfInfoTracker");
var redis = builder.AddAzureRedis("redis").RunAsContainer();

var api = builder.AddProject<Projects.GulfInfoTracker_Api>("api")
                 .WithReference(db)
                 .WithReference(redis)
                 .WaitFor(db)
                 .WaitFor(redis)
                 .WithExternalHttpEndpoints();

builder.AddNpmApp("web", "../GulfInfoTracker.Web")
       .WithReference(api)
       .WaitFor(api)
       .WithHttpEndpoint(env: "PORT")
       .WithExternalHttpEndpoints();

builder.Build().Run();
