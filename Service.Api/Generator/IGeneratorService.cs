using Service.Api.Entities;

namespace Service.Api.Generator;

/// <summary>
/// Интерфейс для запуска юзкейса по обработке учебного курса
/// </summary>
public interface IGeneratorService
{
    /// <summary>
    /// Генерирует один случайный учебный курс
    /// </summary>
    /// <returns>Сгенерированный курс</returns>
    Task<TrainingCourse> ProcessTrainingCourse(int id);
}
