using Amazon.SimpleNotificationService.Util;
using File.Service.Storage;
using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace File.Service.Controllers;

/// <summary>
/// Контроллер для приёма сообщений от SNS-топика
/// </summary>
/// <param name="s3Service">Служба для работы с объектным хранилищем</param>
/// <param name="configuration">Конфигурация приложения</param>
/// <param name="logger">Логгер</param>
[ApiController]
[Route("api/sns")]
public class SnsSubscriberController(
    IS3Service s3Service,
    IConfiguration configuration,
    ILogger<SnsSubscriberController> logger) : ControllerBase
{
    /// <summary>
    /// Адрес LocalStack, используемый для подтверждения подписки SNS изнутри контейнера
    /// </summary>
    private readonly string _confirmationHost = configuration["AWS:Resources:SubscriptionConfirmationHost"]
        ?? throw new KeyNotFoundException("Хост для подтверждения подписки SNS не найден в конфигурации");

    /// <summary>
    /// Порт LocalStack, используемый для подтверждения подписки SNS
    /// </summary>
    private readonly int _confirmationPort = int.TryParse(
            configuration["AWS:Resources:SubscriptionConfirmationPort"], out var port)
        ? port
        : throw new KeyNotFoundException("Порт для подтверждения подписки SNS не найден в конфигурации");

    /// <summary>
    /// Webhook, принимающий уведомления и подтверждения подписки SNS-топика
    /// </summary>
    /// <remarks>
    /// Эндпоинт используется как для приёма уведомлений, так и для подтверждения подписки
    /// при инициализации информационного обмена. В обоих случаях должен возвращать 200.
    /// </remarks>
    [HttpPost]
    [ProducesResponseType(200)]
    public async Task<IActionResult> ReceiveMessage()
    {
        logger.LogInformation("Вызван SNS webhook");
        try
        {
            using var reader = new StreamReader(Request.Body, Encoding.UTF8);
            var jsonContent = await reader.ReadToEndAsync();

            var snsMessage = Message.ParseMessage(jsonContent);

            if (snsMessage.Type == "SubscriptionConfirmation")
            {
                logger.LogInformation("Получен запрос на подтверждение подписки");
                using var httpClient = new HttpClient();
                var builder = new UriBuilder(new Uri(snsMessage.SubscribeURL))
                {
                    Scheme = "http",
                    Host = _confirmationHost,
                    Port = _confirmationPort
                };
                var response = await httpClient.GetAsync(builder.Uri);
                if (!response.IsSuccessStatusCode)
                {
                    var body = await response.Content.ReadAsStringAsync();
                    throw new Exception($"SubscriptionConfirmation вернул {response.StatusCode}: {body}");
                }
                logger.LogInformation("Подписка успешно подтверждена");
                return Ok();
            }

            if (snsMessage.Type == "Notification")
            {
                await s3Service.UploadFile(snsMessage.MessageText);
                logger.LogInformation("Уведомление обработано и сохранено в хранилище");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Исключение при обработке SNS-уведомления");
        }
        return Ok();
    }
}
