using Service.Api.Entities;

namespace Service.Api.Messaging;

/// <summary>
/// Интерфейс службы для отправки сгенерированных учебных курсов в брокер сообщений
/// </summary>
public interface IProducerService
{
    /// <summary>
    /// Отправляет сообщение об учебном курсе в брокер
    /// </summary>
    /// <param name="trainingCourse">Учебный курс</param>
    public Task SendMessage(TrainingCourse trainingCourse);
}
