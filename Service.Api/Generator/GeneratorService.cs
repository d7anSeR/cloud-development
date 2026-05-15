using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Service.Api.Entities;
using Service.Api.Messaging;  // ← добавить

namespace Service.Api.Generator;

/// <summary>
/// Сервис для генерации и кэширования учебных курсов
/// </summary>
public class GeneratorService : IGeneratorService
{
    private readonly IDistributedCache _cache;
    private readonly IProducerService _messagingService;
    private readonly ILogger<GeneratorService> _logger;
    private readonly TimeSpan _cacheExpiration;

    public GeneratorService(
        IDistributedCache cache,
        IProducerService messagingService,
        IConfiguration configuration,
        ILogger<GeneratorService> logger)
    {
        _cache = cache;
        _messagingService = messagingService;
        _logger = logger;
        _cacheExpiration = int.TryParse(configuration["CacheExpiration"], out var sec)
            ? TimeSpan.FromSeconds(sec)
            : TimeSpan.FromSeconds(3600);
    }

    /// <summary>
    /// Генерирует один случайный учебный курс и сохраняет в кэш
    /// </summary>
    public async Task<TrainingCourse?> ProcessTrainingCourse(int id)
    {
        try
        {
            _logger.LogInformation("Начало генерации учебного курса");
            
            var trainingCourse = await GetCourseFromCacheAsync(id);
            if (trainingCourse != null)
            {
                return trainingCourse;
            }
            
            trainingCourse = TrainingCourseGenerator.GenerateOne(id);
            _logger.LogInformation("Курс успешно сгенерирован. ID: {CourseId}", trainingCourse.Id);
            
            // ← ДОБАВИТЬ: отправка в SNS
            await _messagingService.SendMessage(trainingCourse);
            
            await SaveCourseToCacheAsync(trainingCourse);
            return trainingCourse;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при генерации курса c {CourseId}", id);
            return null;
        }
    }

    /// <summary>
    /// Получает курс по ID из кэша
    /// </summary>
    private async Task<TrainingCourse?> GetCourseFromCacheAsync(int id)
    {
        var cachedData = await _cache.GetStringAsync(id.ToString());
        if (string.IsNullOrEmpty(cachedData))
        {
            _logger.LogWarning("Не было найдено курса с ID {CourseId} в кэше", id);
            return null;
        }
        var course = JsonSerializer.Deserialize<TrainingCourse>(cachedData);
        _logger.LogInformation("Курс с ID {CourseId} был найден в кэше", id);
        return course;
    }

    /// <summary>
    /// Сохраняет курс в кэш
    /// </summary>
    private async Task SaveCourseToCacheAsync(TrainingCourse course)
    {
        _logger.LogInformation("Курс с ID: {CourseId} успешно добавлен в кэш", course.Id);
        var jsonData = JsonSerializer.Serialize(course);
        await _cache.SetStringAsync(course.Id.ToString(), jsonData,
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = _cacheExpiration
            });
    }
}