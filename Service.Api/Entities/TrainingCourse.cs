using System.Text.Json.Serialization;

namespace Service.Api.Entities;

/// <summary>
/// Учебный курс
/// </summary>
public class TrainingCourse
{
    /// <summary>
    /// Идентификатор курса в системе
    /// </summary>
    [JsonPropertyName("id")]
    public int Id { get; set; }

    /// <summary>
    /// Наименование курса
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// ФИО преподавателя (фамилия, имя, отчество через пробел)
    /// </summary>
    [JsonPropertyName("teacherFullName")]
    public string TeacherFullName { get; set; } = string.Empty;

    /// <summary>
    /// Дата начала курса
    /// </summary>
    [JsonPropertyName("startDate")]
    public DateOnly StartDate { get; set; }

    /// <summary>
    /// Дата окончания курса
    /// </summary>
    [JsonPropertyName("endDate")]
    public DateOnly? EndDate { get; set; }

    /// <summary>
    /// Максимальное число студентов
    /// </summary>
    [JsonPropertyName("maxStudents")]
    public int MaxStudents { get; set; }

    /// <summary>
    /// Текущее число студентов
    /// </summary>
    [JsonPropertyName("currentStudents")]
    public int? CurrentStudents { get; set; }

    /// <summary>
    /// Выдача сертификата по окончании
    /// </summary>
    [JsonPropertyName("hasCertificate")]
    public bool HasCertificate { get; set; }

    /// <summary>
    /// Стоимость курса
    /// </summary>
    [JsonPropertyName("price")]
    public decimal? Price { get; set; }

    /// <summary>
    /// Рейтинг курса (от 1 до 5)
    /// </summary>
    [JsonPropertyName("rating")]
    public int? Rating { get; set; }
}