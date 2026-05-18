using Bogus;
using Bogus.DataSets;
using Service.Api.Entities;

namespace Service.Api.Generator;

/// <summary>
/// Генератор тестовых данных для учебных курсов
/// </summary>
public static class TrainingCourseGenerator
{
    private static readonly Faker<TrainingCourse> _courseFaker;

    /// <summary>
    /// Инициализирует генератор
    /// </summary>
    static TrainingCourseGenerator()
    {
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
        _courseFaker = new Faker<TrainingCourse>("ru")
            .RuleFor(c => c.Id, f => f.IndexFaker + 1)
            .RuleFor(c => c.Name, f => f.PickRandom(courseNames))
            .RuleFor(c => c.TeacherFullName, f =>
            {
                var gender = f.PickRandom<Name.Gender>();

                var firstName = f.Name.FirstName(gender);
                var lastName = f.Name.LastName(gender);

                var patronymicSuffix = gender == Name.Gender.Male ? "ович" : "овна";
                var patronymic = f.Name.FirstName(Name.Gender.Male) + patronymicSuffix;

                return $"{lastName} {firstName} {patronymic}";
            })
            .RuleFor(c => c.StartDate, f =>
            {
                var daysToStart = f.Random.Int(3, 60);
                return f.Date.SoonDateOnly(daysToStart);
            })
            .RuleFor(c => c.EndDate, (f, c) =>
            {
                var durationDays = f.Random.Int(10, 90);
                return c.StartDate.AddDays(durationDays);
            })
            .RuleFor(c => c.MaxStudents, f => f.Random.Int(5, 30))
            .RuleFor(c => c.CurrentStudents, (f, c) =>
            {
                var today = DateOnly.FromDateTime(DateTime.Now);
                if (c.StartDate <= today)
                {
                    return f.Random.Int(0, c.MaxStudents);
                }
                return f.Random.Int(0, c.MaxStudents / 2);
            })
            .RuleFor(c => c.HasCertificate, f => f.Random.Bool(0.9f))
            .RuleFor(c => c.Price, f =>
            {
                var price = f.Random.Decimal(5000, 150000);
                return Math.Round(price, 2);
            })
            .RuleFor(c => c.Rating, f => f.Random.Int(1, 5));
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
}