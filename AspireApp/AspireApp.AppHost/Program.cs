var builder = DistributedApplication.CreateBuilder(args);

var cache = builder.AddRedis("course-cache")
    .WithImageTag("latest")
    .WithRedisInsight(containerName: "course-insight");

var gateway = builder.AddProject<Projects.Api_Gateway>("api-gateway");

for (var i = 0; i < 5; i++)
{
    var service = builder.AddProject<Projects.Service_Api>($"training-course-api-{i + 1}", launchProfileName: null)
        .WithHttpEndpoint(4000 + i)
        .WithReference(cache, "RedisCache")
        .WaitFor(cache);
    gateway.WaitFor(service);
}


builder.AddProject<Projects.Client_Wasm>("training-course")
    .WaitFor(gateway);
 
 
builder.Build().Run();
