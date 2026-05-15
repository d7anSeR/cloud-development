using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using System.Net;

namespace Event.Sink.Messaging;

/// <summary>
/// Служба для подписки на SNS на старте приложения
/// </summary>
public class SnsSubscriptionService : IHostedService
{
    private readonly IAmazonSimpleNotificationService _snsClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<SnsSubscriptionService> _logger;
    private string? _topicArn;

    public SnsSubscriptionService(
        IAmazonSimpleNotificationService snsClient,
        IConfiguration configuration,
        ILogger<SnsSubscriptionService> logger)
    {
        _snsClient = snsClient;
        _configuration = configuration;
        _logger = logger;
    }

    private async Task<string> GetTopicArnAsync()
    {
        if (_topicArn is not null)
            return _topicArn;

        var topicName = _configuration["AWS:Resources:TopicName"] ?? "course-events-topic";
        var response = await _snsClient.CreateTopicAsync(topicName);
        _topicArn = response.TopicArn;
        return _topicArn;
    }

    /// <summary>
    /// Делает попытку подписаться на топик SNS
    /// </summary>
    public async Task SubscribeEndpoint()
    {
        var topicArn = await GetTopicArnAsync();
        _logger.LogInformation("Sending subscribe request for {topic}", topicArn);
        
        var endpoint = _configuration["AWS:Resources:SNSUrl"];
        
        if (string.IsNullOrEmpty(endpoint))
        {
            _logger.LogError("SNSUrl is not configured");
            return;
        }

        var request = new SubscribeRequest
        {
            TopicArn = topicArn,
            Protocol = "http",
            Endpoint = endpoint,
            ReturnSubscriptionArn = true
        };
        
        var response = await _snsClient.SubscribeAsync(request);
        if (response.HttpStatusCode != HttpStatusCode.OK)
            _logger.LogError("Failed to subscribe to {topic}", topicArn);
        else
            _logger.LogInformation("Subscription request for {topic} is successful, waiting for confirmation", topicArn);
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await SubscribeEndpoint();
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}