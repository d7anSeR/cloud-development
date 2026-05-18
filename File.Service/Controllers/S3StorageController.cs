using File.Service.Storage;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json.Nodes;

namespace File.Service.Controllers;

/// <summary>
/// Контроллер для взаимодействия с объектным хранилищем учебных курсов
/// </summary>
/// <param name="s3Service">Служба для работы с объектным хранилищем</param>
/// <param name="logger">Логгер</param>
[ApiController]
[Route("api/s3")]
public class S3StorageController(IS3Service s3Service, ILogger<S3StorageController> logger) : ControllerBase
{
    /// <summary>
    /// Возвращает список ключей файлов, хранящихся в бакете
    /// </summary>
    [HttpGet]
    [ProducesResponseType(200)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<List<string>>> ListFiles()
    {
        logger.LogInformation("Вызван метод {method} контроллера {controller}", nameof(ListFiles), nameof(S3StorageController));
        try
        {
            var list = await s3Service.GetFileList();
            logger.LogInformation("Получен список из {count} файлов в бакете", list.Count);
            return Ok(list);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Исключение при выполнении {method} в {controller}", nameof(ListFiles), nameof(S3StorageController));
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Возвращает JSON-представление файла, хранящегося в бакете
    /// </summary>
    /// <param name="key">Ключ файла</param>
    [HttpGet("{key}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<JsonNode>> GetFile(string key)
    {
        logger.LogInformation("Вызван метод {method} контроллера {controller}", nameof(GetFile), nameof(S3StorageController));
        try
        {
            var node = await s3Service.DownloadFile(key);
            logger.LogInformation("Получен JSON размером {size} байт", Encoding.UTF8.GetByteCount(node.ToJsonString()));
            return Ok(node);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Исключение при выполнении {method} в {controller}", nameof(GetFile), nameof(S3StorageController));
            return BadRequest(ex.Message);
        }
    }
}
