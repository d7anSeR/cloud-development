using Service.Api.Generator;
using Service.Api.Messaging;
using Amazon.SimpleNotificationService;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddRedisDistributedCache("RedisCache");

// Прямая регистрация SNS клиента (без LocalStack.Client)
builder.Services.AddSingleton<IAmazonSimpleNotificationService>(sp =>
{
    var config = new AmazonSimpleNotificationServiceConfig
    {
        ServiceURL = "http://localhost:4566",
        UseHttp = true,
        AuthenticationRegion = "eu-central-1"
    };
    return new AmazonSimpleNotificationServiceClient(config);
});

builder.Services.AddScoped<IProducerService, SnsPublisherService>();
builder.Services.AddScoped<IGeneratorService, GeneratorService>();

var app = builder.Build();

app.MapDefaultEndpoints();
app.MapGet("/training-course", (IGeneratorService service, int id) => service.ProcessTrainingCourse(id));

app.Run();