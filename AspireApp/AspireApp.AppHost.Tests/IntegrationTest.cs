using Aspire.Hosting;
using Microsoft.Extensions.Logging;
using Service.Api.Entities;
using System.Text.Json;
using Xunit.Abstractions;

namespace AspireApp.AppHost.Tests;

/// <summary>
/// Интеграционные тесты для проверки совместной работы сервисов бекенда
/// </summary>
/// <param name="output">Служба журналирования юнит-тестов</param>
public class IntegrationTest(ITestOutputHelper output) : IAsyncLifetime
{
    private DistributedApplication? _app;

    /// <inheritdoc/>
    public async Task InitializeAsync()
    {
        var cancellationToken = CancellationToken.None;
        IDistributedApplicationTestingBuilder builder = await DistributedApplicationTestingBuilder.CreateAsync<Projects.AspireApp_AppHost>(cancellationToken);
        builder.Configuration["DcpPublisher:RandomizePorts"] = "false";
        builder.Services.AddLogging(logging =>
        {
            logging.AddXUnit(output);
            logging.SetMinimumLevel(LogLevel.Debug);
            logging.AddFilter("Aspire.Hosting.Dcp", LogLevel.Debug);
            logging.AddFilter("Aspire.Hosting", LogLevel.Debug);
        });
        _app = await builder.BuildAsync(cancellationToken);
        await _app.StartAsync(cancellationToken);
    }

    /// <summary>
    /// Проверяет, что вызов гейтвея возвращает сгенерированный учебный курс
    /// и тот же курс через брокер попадает в Minio в виде файла
    /// </summary>
    [Fact]
    public async Task TestPipeline_CourseIsReturnedAndStoredInMinio()
    {
        var random = new Random();
        var id = random.Next(1, 1000);

        using var gatewayClient = _app!.CreateHttpClient("api-gateway", "http");
        using var gatewayResponse = await gatewayClient.GetAsync($"/training-course?id={id}");
        var apiCourse = JsonSerializer.Deserialize<TrainingCourse>(await gatewayResponse.Content.ReadAsStringAsync());

        await Task.Delay(5000);

        using var fileServiceClient = _app!.CreateHttpClient("file-service", "http");
        using var listResponse = await fileServiceClient.GetAsync("/api/s3");
        var fileList = JsonSerializer.Deserialize<List<string>>(await listResponse.Content.ReadAsStringAsync());

        using var fileResponse = await fileServiceClient.GetAsync($"/api/s3/training_course_{id}.json");
        var storedCourse = JsonSerializer.Deserialize<TrainingCourse>(await fileResponse.Content.ReadAsStringAsync());

        Assert.NotNull(fileList);
        Assert.NotNull(apiCourse);
        Assert.NotNull(storedCourse);
        Assert.Equal(id, storedCourse.Id);
        Assert.Equivalent(apiCourse, storedCourse);
    }

    /// <summary>
    /// Проверяет, что повторный запрос того же курса возвращает идентичный результат
    /// </summary>
    [Fact]
    public async Task TestPipeline_RepeatedRequestReturnsCachedCourse()
    {
        var random = new Random();
        var id = random.Next(1001, 2000);

        using var gatewayClient = _app!.CreateHttpClient("api-gateway", "http");

        using var firstResponse = await gatewayClient.GetAsync($"/training-course?id={id}");
        var firstCourse = JsonSerializer.Deserialize<TrainingCourse>(await firstResponse.Content.ReadAsStringAsync());

        using var secondResponse = await gatewayClient.GetAsync($"/training-course?id={id}");
        var secondCourse = JsonSerializer.Deserialize<TrainingCourse>(await secondResponse.Content.ReadAsStringAsync());

        Assert.NotNull(firstCourse);
        Assert.NotNull(secondCourse);
        Assert.Equal(id, firstCourse.Id);
        Assert.Equivalent(firstCourse, secondCourse);
    }

    /// <inheritdoc/>
    public async Task DisposeAsync()
    {
        await _app!.StopAsync();
        await _app.DisposeAsync();
    }
}
