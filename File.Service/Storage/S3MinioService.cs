using Minio;
using Minio.DataModel.Args;
using System.Net;
using System.Text;
using System.Text.Json.Nodes;

namespace File.Service.Storage;

/// <summary>
/// Служба для манипуляции файлами учебных курсов в Minio
/// </summary>
/// <param name="client">Клиент Minio</param>
/// <param name="configuration">Конфигурация приложения</param>
/// <param name="logger">Логгер</param>
public class S3MinioService(
    IMinioClient client,
    IConfiguration configuration,
    ILogger<S3MinioService> logger) : IS3Service
{
    private readonly string _bucketName = configuration["AWS:Resources:MinioBucketName"]
        ?? throw new KeyNotFoundException("Имя бакета Minio не найдено в конфигурации");

    /// <inheritdoc/>
    public async Task<List<string>> GetFileList()
    {
        var list = new List<string>();
        var request = new ListObjectsArgs()
            .WithBucket(_bucketName)
            .WithPrefix("")
            .WithRecursive(true);
        logger.LogInformation("Запрос списка файлов в {bucket}", _bucketName);
        var responseList = client.ListObjectsEnumAsync(request);

        if (responseList == null)
            logger.LogWarning("Из {bucket} получен пустой ответ", _bucketName);

        await foreach (var response in responseList!)
            list.Add(response.Key);
        return list;
    }

    /// <inheritdoc/>
    public async Task<bool> UploadFile(string fileData)
    {
        var rootNode = JsonNode.Parse(fileData) ?? throw new ArgumentException("Переданная строка не является валидным JSON");
        var id = rootNode["id"]?.GetValue<int>() ?? throw new ArgumentException("JSON имеет некорректную структуру");

        var bytes = Encoding.UTF8.GetBytes(fileData);
        using var stream = new MemoryStream(bytes);
        stream.Seek(0, SeekOrigin.Begin);

        logger.LogInformation("Загрузка учебного курса {file} в {bucket}", id, _bucketName);
        var request = new PutObjectArgs()
            .WithBucket(_bucketName)
            .WithStreamData(stream)
            .WithObjectSize(bytes.Length)
            .WithObject($"training_course_{id}.json");

        var response = await client.PutObjectAsync(request);

        if (response.ResponseStatusCode != HttpStatusCode.OK)
        {
            logger.LogError("Не удалось загрузить учебный курс {file}: {code}", id, response.ResponseStatusCode);
            return false;
        }
        logger.LogInformation("Учебный курс {file} успешно загружен в {bucket}", id, _bucketName);
        return true;
    }

    /// <inheritdoc/>
    public async Task<JsonNode> DownloadFile(string key)
    {
        logger.LogInformation("Скачивание {file} из {bucket}", key, _bucketName);

        try
        {
            var memoryStream = new MemoryStream();

            var request = new GetObjectArgs()
                .WithBucket(_bucketName)
                .WithObject(key)
                .WithCallbackStream(async (stream, cancellationToken) =>
                {
                    await stream.CopyToAsync(memoryStream, cancellationToken);
                    memoryStream.Seek(0, SeekOrigin.Begin);
                });

            var response = await client.GetObjectAsync(request);

            if (response == null)
            {
                logger.LogError("Не удалось скачать {file}", key);
                throw new InvalidOperationException($"Ошибка при скачивании {key} — объект равен null");
            }
            using var reader = new StreamReader(memoryStream, Encoding.UTF8);
            return JsonNode.Parse(reader.ReadToEnd())
                ?? throw new InvalidOperationException("Скачанный документ не является валидным JSON");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Исключение при скачивании файла {file}", key);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task EnsureBucketExists()
    {
        logger.LogInformation("Проверка существования бакета {bucket}", _bucketName);
        try
        {
            var existsRequest = new BucketExistsArgs().WithBucket(_bucketName);
            var exists = await client.BucketExistsAsync(existsRequest);
            if (!exists)
            {
                logger.LogInformation("Создание бакета {bucket}", _bucketName);
                var createRequest = new MakeBucketArgs().WithBucket(_bucketName);
                await client.MakeBucketAsync(createRequest);
                return;
            }
            logger.LogInformation("Бакет {bucket} существует", _bucketName);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Необработанное исключение при проверке бакета {bucket}", _bucketName);
            throw;
        }
    }
}
