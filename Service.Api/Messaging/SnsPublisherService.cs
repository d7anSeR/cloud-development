using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using Service.Api.Entities;
using System.Net;
using System.Text.Json;

namespace Service.Api.Messaging;

/// <summary>
/// Служба для публикации сообщений об учебных курсах в SNS-топик
/// </summary>
/// <param name="client">Клиент SNS</param>
/// <param name="configuration">Конфигурация приложения</param>
/// <param name="logger">Логгер</param>
public class SnsPublisherService(
    IAmazonSimpleNotificationService client,
    IConfiguration configuration,
    ILogger<SnsPublisherService> logger) : IProducerService
{
    private readonly string _topicArn = configuration["AWS:Resources:SNSTopicArn"]
        ?? throw new KeyNotFoundException("SNS topic ARN was not found in configuration");

    /// <inheritdoc/>
    public async Task SendMessage(TrainingCourse trainingCourse)
    {
        try
        {
            var json = JsonSerializer.Serialize(trainingCourse);
            var request = new PublishRequest
            {
                Message = json,
                TopicArn = _topicArn
            };
            var response = await client.PublishAsync(request);
            if (response.HttpStatusCode == HttpStatusCode.OK)
                logger.LogInformation("Учебный курс {id} был отправлен в SNS-топик", trainingCourse.Id);
            else
                throw new Exception($"SNS вернул {response.HttpStatusCode}");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Не удалось отправить учебный курс {id} в SNS-топик", trainingCourse.Id);
        }
    }
}
