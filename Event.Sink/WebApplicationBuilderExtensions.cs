using Amazon.SimpleNotificationService;
using Event.Sink.Messaging;
using Event.Sink.Storage;
using LocalStack.Client.Extensions;

namespace Event.Sink;

/// <summary>
/// Экстеншен для добавления различных служб в DI в зависимости от конфигурации приложения
/// </summary>
internal static class WebApplicationBuilderExtensions
{
    /// <summary>
    /// Регистрирует клиентские службы для работы с брокером сообщений
    /// </summary>
    public static WebApplicationBuilder AddConsumer(this WebApplicationBuilder builder)
    {
        builder.Services.AddLocalStack(builder.Configuration);
        return builder.Configuration.GetSection("Settings")["MessageBroker"] switch
        {
            "SNS" => builder.AddSnsSubscriber(),
            _ => throw new KeyNotFoundException("Invalid broker type")
        };
    }

    /// <summary>
    /// Регистрирует службы для работы с SNS
    /// </summary>
    private static WebApplicationBuilder AddSnsSubscriber(this WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<SnsSubscriptionService>();
        builder.Services.AddAwsService<IAmazonSimpleNotificationService>();
        return builder;
    }

    /// <summary>
    /// Регистрирует клиентские службы для работы с объектным хранилищем
    /// </summary>
    public static WebApplicationBuilder AddS3(this WebApplicationBuilder builder)
    {
        return builder.Configuration.GetSection("Settings")["S3Hosting"] switch
        {
            "Minio" => builder.AddMinio(),
            _ => throw new KeyNotFoundException("Invalid s3 hosting type")
        };
    }

    /// <summary>
    /// Регистрирует службы для работы с S3 по клиентскому API Minio
    /// </summary>
    private static WebApplicationBuilder AddMinio(this WebApplicationBuilder builder)
    {
        builder.AddMinioClient("course-minio");
        builder.Services.AddScoped<IS3Service, S3MinioService>();
        return builder;
    }
}