using Event.Sink.Messaging;
using Event.Sink.Storage;

namespace Event.Sink;

/// <summary>
/// Экстеншен для инициализации сервисов
/// </summary>
internal static class WebApplicationExtensions
{
    /// <summary>
    /// Запускает подписку на SNS
    /// </summary>
    public static async Task<WebApplication> UseConsumer(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var subscriptionService = scope.ServiceProvider.GetRequiredService<SnsSubscriptionService>();
        await subscriptionService.SubscribeEndpoint();
        return app;
    }

    /// <summary>
    /// Создаёт бакет в Minio если его нет
    /// </summary>
    public static async Task<WebApplication> UseS3(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var s3Service = scope.ServiceProvider.GetRequiredService<IS3Service>();
        await s3Service.EnsureBucketExists();
        return app;
    }
}