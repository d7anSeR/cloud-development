using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Service.Api.Entities;

namespace Service.Api.Generator;

/// <summary>
/// Сервис для генерации и кэширования учебных курсов
/// </summary>
public class GeneratorService (IDistributedCache cache, IConfiguration configuration, ILogger<GeneratorService> logger): IGeneratorService
{
    private readonly TimeSpan _cacheExpiration = int.TryParse(configuration["CacheExpiration"], out var sec)
        ? TimeSpan.FromSeconds(sec)
        : TimeSpan.FromSeconds(3600);

    /// <summary>
    /// Генерирует один случайный учебный курс и сохраняет в кэш
    /// </summary>
    public async Task<TrainingCourse?> ProcessTrainingCourse(int id)
    {
        try
        {
            logger.LogInformation("Начало генерации учебного курса");
            var trainingCourse = await GetCourseFromCacheAsync(id);
            if (trainingCourse != null)
            {
                return trainingCourse;
            }
            trainingCourse = TrainingCourseGenerator.GenerateOne(id);
            logger.LogInformation("Курс успешно сгенерирован. ID: {CourseId}", trainingCourse.Id);
            await SaveCourseToCacheAsync(trainingCourse);
            return trainingCourse;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка при генерации курса c {CourseId}", id);
            return null;
        }
    }

    /// <summary>
    /// Получает курс по ID из кэша
    /// </summary>
    private async Task<TrainingCourse?> GetCourseFromCacheAsync(int id)
    {
        var cachedData = await cache.GetStringAsync(id.ToString());
        if (string.IsNullOrEmpty(cachedData))
        {
            logger.LogWarning("Не было найдено курса с ID {CourseId} в кэше", id);
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
        logger.LogInformation("Курс с ID: {CourseId} успешно добавлен в кэш", course.Id);
        var jsonData = JsonSerializer.Serialize(course);
        await cache.SetStringAsync(course.Id.ToString(), jsonData,
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = _cacheExpiration
            });
    }
}