using Microsoft.Extensions.Caching.Distributed;
using Service.Api.Entities;
using Service.Api.Messaging;
using System.Text.Json;

namespace Service.Api.Generator;

/// <summary>
/// Сервис для генерации и кэширования учебных курсов с публикацией в брокер
/// </summary>
/// <param name="cache">Распределённый кэш</param>
/// <param name="producer">Служба отправки сообщений в брокер</param>
/// <param name="configuration">Конфигурация приложения</param>
/// <param name="logger">Логгер</param>
public class GeneratorService(
    IDistributedCache cache,
    IProducerService producer,
    IConfiguration configuration,
    ILogger<GeneratorService> logger) : IGeneratorService
{
    private readonly TimeSpan _cacheExpiration = int.TryParse(configuration["CacheExpiration"], out var sec)
        ? TimeSpan.FromSeconds(sec)
        : TimeSpan.FromSeconds(3600);

    /// <summary>
    /// Возвращает курс из кэша или генерирует новый, публикует его в брокер и кладёт в кэш
    /// </summary>
    public async Task<TrainingCourse?> ProcessTrainingCourse(int id)
    {
        try
        {
            logger.LogInformation("Начало обработки учебного курса {id}", id);
            var trainingCourse = await GetCourseFromCacheAsync(id);
            if (trainingCourse != null)
            {
                return trainingCourse;
            }
            trainingCourse = TrainingCourseGenerator.GenerateOne(id);
            logger.LogInformation("Курс успешно сгенерирован. ID: {CourseId}", trainingCourse.Id);
            await producer.SendMessage(trainingCourse);
            await SaveCourseToCacheAsync(trainingCourse);
            return trainingCourse;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка при обработке курса {CourseId}", id);
            return null;
        }
    }

    /// <summary>
    /// Получает курс по идентификатору из кэша
    /// </summary>
    private async Task<TrainingCourse?> GetCourseFromCacheAsync(int id)
    {
        var cachedData = await cache.GetStringAsync(id.ToString());
        if (string.IsNullOrEmpty(cachedData))
        {
            logger.LogWarning("Курс с ID {CourseId} не найден в кэше", id);
            return null;
        }
        var course = JsonSerializer.Deserialize<TrainingCourse>(cachedData);
        logger.LogInformation("Курс с ID {CourseId} был найден в кэше", id);
        return course;
    }

    /// <summary>
    /// Сохраняет курс в кэш
    /// </summary>
    private async Task SaveCourseToCacheAsync(TrainingCourse course)
    {
        logger.LogInformation("Курс с ID {CourseId} добавлен в кэш", course.Id);
        var jsonData = JsonSerializer.Serialize(course);
        await cache.SetStringAsync(course.Id.ToString(), jsonData,
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = _cacheExpiration
            });
    }
}
