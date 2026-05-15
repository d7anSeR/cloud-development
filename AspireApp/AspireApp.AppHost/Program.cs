using Amazon;
using Aspire.Hosting.AWS;
using Aspire.Hosting.MinIO;

var builder = DistributedApplication.CreateBuilder(args);

var cache = builder.AddRedis("course-cache")
    .WithImageTag("latest")
    .WithRedisInsight(containerName: "course-insight");

var gateway = builder.AddProject<Projects.Api_Gateway>("api-gateway");

var awsConfig = builder.AddAWSSDKConfig()
    .WithProfile("default")
    .WithRegion(RegionEndpoint.EUCentral1);

var brokerType = "SNS";
var s3Hosting = "Minio";

var minio = builder.AddMinioContainer("course-minio")
    .WithDataVolume("course-minio-data")
    .WithBucket("course-bucket");

var awsResources = builder.AddAWSCloudFormationTemplate(
    "course-aws-resources", 
    "CloudFormation/course-template-sns.yaml", 
    "course")
    .WithReference(awsConfig);

var sink = builder.AddProject<Projects.Event_Sink>("course-event-sink")
    .WithReference(awsResources)
    .WithReference(minio)
    .WithEnvironment("Settings__MessageBroker", brokerType)
    .WithEnvironment("Settings__S3Hosting", s3Hosting)
    .WithEnvironment("AWS__Resources__SNSUrl", "http://host.docker.internal:5280/api/sns")
    .WithEnvironment("AWS__Resources__MinioBucketName", "course-bucket")
    .WaitFor(awsResources)
    .WaitFor(minio);

for (var i = 0; i < 5; i++)
{
    var service = builder.AddProject<Projects.Service_Api>($"training-course-api-{i + 1}", launchProfileName: null)
        .WithHttpEndpoint(4000 + i)
        .WithReference(cache, "RedisCache")
        .WithReference(awsResources)
        .WithEnvironment("Settings__MessageBroker", brokerType)
        .WaitFor(cache)
        .WaitFor(awsResources);
    
    gateway.WaitFor(service);
}

builder.AddProject<Projects.Client_Wasm>("training-course")
    .WaitFor(gateway);

builder.Build().Run();