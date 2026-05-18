using Amazon;
using Aspire.Hosting.LocalStack.Container;

var builder = DistributedApplication.CreateBuilder(args);

var cache = builder.AddRedis("course-cache")
    .WithImageTag("latest")
    .WithRedisInsight(containerName: "course-insight");

var gateway = builder.AddProject<Projects.Api_Gateway>("api-gateway");

var awsConfig = builder.AddAWSSDKConfig()
    .WithProfile("default")
    .WithRegion(RegionEndpoint.EUCentral1);

var localstack = builder
    .AddLocalStack("course-localstack", awsConfig: awsConfig, configureContainer: container =>
    {
        container.Lifetime = ContainerLifetime.Session;
        container.DebugLevel = 1;
        container.LogLevel = LocalStackLogLevel.Debug;
        container.Port = 4566;
        container.AdditionalEnvironmentVariables.Add("DEBUG", "1");
        container.AdditionalEnvironmentVariables.Add("SNS_CERT_URL_HOST", "sns.eu-central-1.amazonaws.com");
    });

var awsResources = builder.AddAWSCloudFormationTemplate("resources", "CloudFormation/training-course-template.yaml", "training-course")
    .WithReference(awsConfig);

var minio = builder.AddMinioContainer("course-minio");

for (var i = 0; i < 5; i++)
{
    var service = builder.AddProject<Projects.Service_Api>($"training-course-api-{i + 1}", launchProfileName: null)
        .WithHttpEndpoint(4000 + i)
        .WithReference(cache, "RedisCache")
        .WithReference(awsResources)
        .WaitFor(cache)
        .WaitFor(awsResources);
    gateway.WaitFor(service);
}

builder.AddProject<Projects.Client_Wasm>("training-course")
    .WaitFor(gateway);

builder.AddProject<Projects.File_Service>("file-service", launchProfileName: null)
    .WithHttpEndpoint(5280)
    .WithReference(awsResources)
    .WithReference(minio)
    .WithEnvironment("AWS__Resources__MinioBucketName", "training-course-bucket")
    .WithEnvironment("AWS__Resources__SNSUrl", $"http://host.docker.internal:5280/api/sns")
    .WaitFor(awsResources)
    .WaitFor(minio);

builder.UseLocalStack(localstack);

builder.Build().Run();
