using Amazon.SimpleNotificationService;
using LocalStack.Client;
using Service.Api.Messaging;

namespace Service.Api;

internal static class WebApplicationBuilderExtensions
{
    public static WebApplicationBuilder AddProducer(this WebApplicationBuilder builder)
    {
        builder.Services.AddLocalStack(builder.Configuration);
        builder.Services.AddScoped<IProducerService, SnsPublisherService>();
        builder.Services.AddAwsService<IAmazonSimpleNotificationService>();
        return builder;
    }
}