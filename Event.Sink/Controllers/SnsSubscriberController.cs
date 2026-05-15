using Amazon.SimpleNotificationService.Util;
using Event.Sink.Storage;
using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace Event.Sink.Controllers;

/// <summary>
/// Контроллер для приема сообщений от SNS
/// </summary>
[ApiController]
[Route("api/sns")]
public class SnsSubscriberController : ControllerBase
{
    private readonly IS3Service _s3Service;
    private readonly ILogger<SnsSubscriberController> _logger;
    private readonly HashSet<string> _processedMessages = new();

    public SnsSubscriberController(IS3Service s3Service, ILogger<SnsSubscriberController> logger)
    {
        _s3Service = s3Service;
        _logger = logger;
    }

    /// <summary>
    /// Вебхук, который получает оповещения из SNS топика
    /// </summary>
    [HttpPost]
    [ProducesResponseType(200)]
    public async Task<IActionResult> ReceiveMessage()
    {
        _logger.LogInformation("SNS webhook was called");
        
        try
        {
            using var reader = new StreamReader(Request.Body, Encoding.UTF8);
            string jsonContent = await reader.ReadToEndAsync();

            var snsMessage = Message.ParseMessage(jsonContent);

            if (snsMessage.Type == "SubscriptionConfirmation")
            {
                _logger.LogInformation("SubscriptionConfirmation was received");
                
                using var httpClient = new HttpClient();
                var response = await httpClient.GetAsync(snsMessage.SubscribeURL);
                
                if (!response.IsSuccessStatusCode)
                {
                    var body = await response.Content.ReadAsStringAsync();
                    _logger.LogError("SubscriptionConfirmation failed: {StatusCode} - {Body}", response.StatusCode, body);
                    throw new Exception($"SubscriptionConfirmation returned {response.StatusCode}");
                }
                
                _logger.LogInformation("Subscription was successfully confirmed");
                return Ok();
            }

            if (snsMessage.Type == "Notification")
            {
                if (_processedMessages.Contains(snsMessage.MessageId))
                {
                    _logger.LogInformation("Duplicate message {MessageId} ignored", snsMessage.MessageId);
                    return Ok();
                }
                
                _processedMessages.Add(snsMessage.MessageId);
                
                if (_processedMessages.Count > 1000)
                {
                    _processedMessages.Clear();
                }
                
                _logger.LogInformation("Processing notification {MessageId}", snsMessage.MessageId);
                await _s3Service.UploadFile(snsMessage.MessageText);
                _logger.LogInformation("Notification {MessageId} was successfully processed", snsMessage.MessageId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while processing SNS notifications");
        }
        
        return Ok();
    }
}