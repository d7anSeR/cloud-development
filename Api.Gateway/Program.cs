using Api.Gateway.LoadBalancer;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddServiceDiscovery();
builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);
builder.Services.AddOcelot()
    .AddCustomLoadBalancer<WeightedRandom>((serviceProvider, _, discoveryProvider) => 
    {
        var configuration = serviceProvider.GetRequiredService<IConfiguration>();
        return new WeightedRandom(discoveryProvider.GetAsync, configuration);
    });

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(
                "https://localhost:5127",
                "http://localhost:5127",
                "https://localhost:7282"
              )
              .WithMethods("GET")
              .AllowAnyHeader();
    });
});
var app = builder.Build();
app.UseCors();
await app.UseOcelot();
app.Run();