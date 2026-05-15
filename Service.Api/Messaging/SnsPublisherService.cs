using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using Service.Api.Entities;
using System.Net;
using System.Text.Json;

namespace Service.Api.Messaging;

/// <summary>
/// Служба для отправки сообщений в SNS
/// </summary>
public class SnsPublisherService : IProducerService
{
    private readonly IAmazonSimpleNotificationService _client;
    private readonly IConfiguration _configuration;
    private readonly ILogger<SnsPublisherService> _logger;
    private string? _topicArn;

    public SnsPublisherService(
        IAmazonSimpleNotificationService client,
        IConfiguration configuration,
        ILogger<SnsPublisherService> logger)
    {
        _client = client;
        _configuration = configuration;
        _logger = logger;
    }

    private async Task<string> GetTopicArnAsync()
    {
        if (_topicArn is not null)
            return _topicArn;

        var topicName = _configuration["AWS:Resources:TopicName"] ?? "course-events-topic";
        var response = await _client.CreateTopicAsync(topicName);
        _topicArn = response.TopicArn;
        return _topicArn;
    }

    public async Task SendMessage(TrainingCourse course)
    {
        try
        {
            var topicArn = await GetTopicArnAsync();
            var json = JsonSerializer.Serialize(course);
            var request = new PublishRequest
            {
                Message = json,
                TopicArn = topicArn
            };
            var response = await _client.PublishAsync(request);
            if (response.HttpStatusCode == HttpStatusCode.OK)
                _logger.LogInformation("Course {id} was sent to sink via SNS", course.Id);
            else
                throw new Exception($"SNS returned {response.HttpStatusCode}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unable to send course through SNS topic");
        }
    }
}