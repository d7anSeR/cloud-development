using Amazon.SimpleNotificationService;
using LocalStack.Client.Extensions;
using Service.Api.Generator;
using Service.Api.Messaging;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddRedisDistributedCache("RedisCache");

builder.Services.AddLocalStack(builder.Configuration);
builder.Services.AddAwsService<IAmazonSimpleNotificationService>();
builder.Services.AddScoped<IProducerService, SnsPublisherService>();
builder.Services.AddScoped<IGeneratorService, GeneratorService>();

var app = builder.Build();
app.MapDefaultEndpoints();
app.MapGet("/training-course", (IGeneratorService service, int id) => service.ProcessTrainingCourse(id));
app.Run();
