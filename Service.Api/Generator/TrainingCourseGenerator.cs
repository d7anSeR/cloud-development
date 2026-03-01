using Bogus;
using Service.Api.Entities;

namespace Service.Api.Generator;

/// <summary>
/// Генератор тестовых данных для учебных курсов
/// </summary>
public static class TrainingCourseGenerator
{
    private static readonly Faker<TrainingCourse> _courseFaker;
    private static int _idCounter = 1;

    /// <summary>
    /// Инициализирует генератор
    /// </summary>
    static TrainingCourseGenerator()
    {
        // Справочник наименований курсов
        var courseNames = new[]
        {
            "ASP.NET Core разработка",
            "Blazor WebAssembly",
            "Entity Framework Core",
            "React для начинающих",
            "Angular продвинутый",
            "Python для анализа данных",
            "Java Spring Boot",
            "Go для микросервисов",
            "Docker и Kubernetes",
            "SQL оптимизация запросов",
            "TypeScript с нуля",
            "Vue.js практикум",
            "C# алгоритмы",
            "Azure DevOps",
            "Git и CI/CD"
        };

        var firstNames = new[] { "Иван", "Петр", "Сергей", "Анна", "Мария", "Елена", "Дмитрий", "Алексей", "Ольга", "Наталья" };
        var lastNames = new[] { "Иванов", "Петров", "Сидоров", "Смирнов", "Кузнецов", "Попов", "Васильев", "Соколов", "Михайлов", "Федоров" };
        var patronymics = new[] { "Иванович", "Петрович", "Сергеевич", "Алексеевич", "Дмитриевич", "Андреевич", "Михайловна", "Александровна", "Владимировна", "Павловна" };

        _courseFaker = new Faker<TrainingCourse>("ru")
            .RuleFor(c => c.Id, f => _idCounter++)
            .RuleFor(c => c.Name, f => f.PickRandom(courseNames))
            .RuleFor(c => c.TeacherFullName, f =>
            {
                var lastName = f.PickRandom(lastNames);
                var firstName = f.PickRandom(firstNames);
                var patronymic = f.PickRandom(patronymics);
                return $"{lastName} {firstName} {patronymic}";
            })
            .RuleFor(c => c.StartDate, f =>
            {
                var daysToStart = f.Random.Int(3, 60);
                return DateOnly.FromDateTime(DateTime.Now.AddDays(daysToStart));
            })
            .RuleFor(c => c.EndDate, (f, c) =>
            {
                if (f.Random.Bool(0.8f))
                {
                    var durationDays = f.Random.Int(10, 90);
                    return c.StartDate.AddDays(durationDays);
                }
                return null;
            })
            .RuleFor(c => c.MaxStudents, f => f.Random.Int(5, 30))
            .RuleFor(c => c.CurrentStudents, (f, c) =>
            {
                var today = DateOnly.FromDateTime(DateTime.Now);
                if (c.StartDate <= today)
                {
                    return f.Random.Int(0, c.MaxStudents);
                }
                return f.Random.Bool(0.7f) ? (int?)null : 0;
            })
            .RuleFor(c => c.HasCertificate, f => f.Random.Bool(0.9f))
            .RuleFor(c => c.Price, f =>
            {
                if (f.Random.Bool(0.15f))
                {
                    return null;
                }
                var price = f.Random.Decimal(5000, 150000);
                return Math.Round(price, 2);
            })
            .RuleFor(c => c.Rating, f =>
            {
                if (f.Random.Bool(0.1f)) return null;
                var rand = f.Random.Int(1, 100);
                if (rand <= 5) return 1;
                if (rand <= 15) return 2;
                if (rand <= 35) return 3;
                if (rand <= 70) return 4;
                return 5;
            });
    }

    /// <summary>
    /// Генерирует один учебный курс с конкретным id
    /// </summary>
    public static TrainingCourse GenerateOne(int id)
    {
        var course = _courseFaker.Generate();
        course.Id = id;
        return course;
    }

    /// <summary>
    /// Генерирует один учебный курс
    /// </summary>
    public static TrainingCourse GenerateOne()
    {
        return _courseFaker.Generate();
    }
}