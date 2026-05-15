using Service.Api.Generator;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddRedisDistributedCache("RedisCache");
builder.Services.AddScoped<IGeneratorService, GeneratorService>();


var app = builder.Build();
app.MapDefaultEndpoints();
app.MapGet("/training-course", (IGeneratorService service, int id) => service.ProcessTrainingCourse(id));
app.Run();
