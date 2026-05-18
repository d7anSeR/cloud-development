using System.Text.Json.Nodes;

namespace File.Service.Storage;

/// <summary>
/// Интерфейс службы для манипуляции файлами в объектном хранилище
/// </summary>
public interface IS3Service
{
    /// <summary>
    /// Сериализует и отправляет файл с учебным курсом в хранилище
    /// </summary>
    /// <param name="fileData">Строковое представление сохраняемого файла (JSON)</param>
    /// <returns>Признак успешной загрузки</returns>
    public Task<bool> UploadFile(string fileData);

    /// <summary>
    /// Получает список ключей всех файлов из хранилища
    /// </summary>
    public Task<List<string>> GetFileList();

    /// <summary>
    /// Получает JSON-представление файла из хранилища
    /// </summary>
    /// <param name="filePath">Ключ файла в бакете</param>
    public Task<JsonNode> DownloadFile(string filePath);

    /// <summary>
    /// Создаёт бакет в хранилище, если он ещё не существует
    /// </summary>
    public Task EnsureBucketExists();
}
