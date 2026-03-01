using Service.Api.Generator;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddRedisDistributedCache("RedisCache");
builder.Services.AddScoped<IGeneratorService, GeneratorService>();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("https://localhost:3000")
              .WithMethods("GET")
              .AllowAnyHeader();
    });
});

var app = builder.Build();
app.MapDefaultEndpoints();
app.MapGet("/training-course", (IGeneratorService service, int id) => service.ProcessTrainingCourse(id));
app.UseCors();
app.Run();
