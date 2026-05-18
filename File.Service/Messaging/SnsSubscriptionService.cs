using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using System.Net;

namespace File.Service.Messaging;

/// <summary>
/// Служба, оформляющая подписку файлового сервиса на SNS-топик при старте приложения
/// </summary>
/// <param name="snsClient">Клиент SNS</param>
/// <param name="configuration">Конфигурация приложения</param>
/// <param name="logger">Логгер</param>
public class SnsSubscriptionService(
    IAmazonSimpleNotificationService snsClient,
    IConfiguration configuration,
    ILogger<SnsSubscriptionService> logger)
{
    private readonly string _topicArn = configuration["AWS:Resources:SNSTopicArn"]
        ?? throw new KeyNotFoundException("ARN SNS-топика не найден в конфигурации");

    /// <summary>
    /// Отправляет запрос на подписку HTTP-эндпоинта файлового сервиса в SNS-топик
    /// </summary>
    public async Task SubscribeEndpoint()
    {
        logger.LogInformation("Отправка запроса на подписку для топика {topic}", _topicArn);
        var endpoint = configuration["AWS:Resources:SNSUrl"];

        var request = new SubscribeRequest
        {
            TopicArn = _topicArn,
            Protocol = "http",
            Endpoint = endpoint,
            ReturnSubscriptionArn = true
        };
        var response = await snsClient.SubscribeAsync(request);
        if (response.HttpStatusCode != HttpStatusCode.OK)
            logger.LogError("Не удалось подписаться на топик {topic}", _topicArn);
        else
            logger.LogInformation("Запрос на подписку для {topic} успешно отправлен, ожидается подтверждение", _topicArn);
    }
}
