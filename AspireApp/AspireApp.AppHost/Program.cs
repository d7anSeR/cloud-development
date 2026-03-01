var builder = DistributedApplication.CreateBuilder(args);

var cache = builder.AddRedis("course-cache")
    .WithImageTag("latest")
    .WithContainerRuntimeArgs("--platform", "linux/arm64")
    .WithRedisInsight(containerName: "course-insight");

var service = builder.AddProject<Projects.Service_Api>("service-api")
    .WithReference(cache, "RedisCache")
    .WaitFor(cache);

builder.AddProject<Projects.Client_Wasm>("training-course")
    .WaitFor(service);
 
builder.Build().Run();
