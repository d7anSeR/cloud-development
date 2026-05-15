using Service.Api.Entities;

namespace Service.Api.Messaging;

/// <summary>
/// Интерфейс службы для отправки генерируемых курсов в брокер сообщений
/// </summary>
public interface IProducerService
{
    /// <summary>
    /// Отправляет сообщение в брокер
    /// </summary>
    /// <param name="course">Учебный курс</param>
    public Task SendMessage(TrainingCourse course);
}