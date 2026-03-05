using Bogus;
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
        var maleFirstNames = new[] { "Иван", "Петр", "Сергей", "Дмитрий", "Алексей", "Михаил", "Андрей", "Николай", "Владимир", "Павел" };
        var femaleFirstNames = new[] { "Анна", "Мария", "Елена", "Ольга", "Наталья", "Татьяна", "Ирина", "Светлана", "Екатерина", "Юлия" };
        var lastNames = new[] { "Иванов", "Петров", "Сидоров", "Смирнов", "Кузнецов", "Попов", "Васильев", "Соколов", "Михайлов", "Федоров" };
        
        var malePatronymics = new[] { "Иванович", "Петрович", "Сергеевич", "Алексеевич", "Дмитриевич", "Андреевич", "Михайлович", "Александрович", "Владимирович", "Павлович" };
        
        var femalePatronymics = new[] { "Ивановна", "Петровна", "Сергеевна", "Алексеевна", "Дмитриевна", "Андреевна", "Михайловна", "Александровна", "Владимировна", "Павловна" };

        _courseFaker = new Faker<TrainingCourse>("ru")
            .RuleFor(c => c.Id, f => f.IndexFaker + 1)
            .RuleFor(c => c.Name, f => f.PickRandom(courseNames))
            .RuleFor(c => c.TeacherFullName, f =>
            {
                var isMale = f.Random.Bool();
                
                string lastName = f.PickRandom(lastNames);
                string firstName;
                string patronymic;
                
                if (isMale)
                {
                    firstName = f.PickRandom(maleFirstNames);
                    patronymic = f.PickRandom(malePatronymics);
                    
                    return $"{lastName} {firstName} {patronymic}";
                }
                else
                {
                    firstName = f.PickRandom(femaleFirstNames);
                    patronymic = f.PickRandom(femalePatronymics);
                
                    string femaleLastName = lastName.EndsWith("ов") || lastName.EndsWith("ев") 
                        ? lastName + "а" 
                        : lastName + "а";
                    
                    return $"{femaleLastName} {firstName} {patronymic}";
                }
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