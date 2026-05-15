using Event.Sink.Messaging;
using Event.Sink.Storage;
using Amazon.SimpleNotificationService;
using Minio;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Прямая регистрация SNS клиента
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

// Регистрация Minio клиента
builder.Services.AddSingleton<IMinioClient>(sp =>
{
    var client = new MinioClient()
        .WithEndpoint("localhost", 9000)
        .WithCredentials("minioadmin", "minioadmin")
        .WithSSL(false)
        .Build();
    return client;
});

builder.Services.AddScoped<SnsSubscriptionService>();
builder.Services.AddHostedService(sp => sp.GetRequiredService<SnsSubscriptionService>());
builder.Services.AddScoped<IS3Service, S3MinioService>();

var app = builder.Build();

// Инициализация
using (var scope = app.Services.CreateScope())
{
    var subscriptionService = scope.ServiceProvider.GetRequiredService<SnsSubscriptionService>();
    await subscriptionService.SubscribeEndpoint();
    
    var s3Service = scope.ServiceProvider.GetRequiredService<IS3Service>();
    await s3Service.EnsureBucketExists();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();
app.Run();