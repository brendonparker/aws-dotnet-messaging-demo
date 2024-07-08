using Amazon.SimpleNotificationService;
using Amazon.SQS;
using LearnAwsMessaging.Api.LocalDevelopment;

namespace LearnAwsMessaging.Api;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddLocalDevelopmentServices(this IServiceCollection services)
    {
        services.AddSingleton<IAmazonSQS, SQSClient>();
        services.AddSingleton<IAmazonSimpleNotificationService, SNSClient>();
        services.AddHostedService<LocalDevBackgroundService>();
        return services;
    }
}