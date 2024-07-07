using System.Reflection;
using AWS.Messaging;
using AWS.Messaging.Configuration;
using AWS.Messaging.Publishers.SQS;
using Microsoft.Extensions.DependencyInjection;

namespace LearnAwsMessaging.Consumer;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAWSMessagingCustomizations(this IServiceCollection services)
    {
        services.AddSingleton<IMessagePublisher>(sr =>
        {
            var concreteMyService = services.First(_ => _.ServiceType == typeof(IMessagePublisher));
            var instance = ActivatorUtilities.CreateInstance(sr, concreteMyService.ImplementationType!);
            if (instance is IMessagePublisher messagePublisher)
            {
                var field = messagePublisher.GetType()
                    .GetField("_publisherTypeMapping", BindingFlags.Instance | BindingFlags.NonPublic);
                if (field is null)
                    throw new InvalidOperationException("_publisherTypeMapping no longer exists!");
                
                var typeMappings = field.GetValue(messagePublisher) as Dictionary<string, Type>;
                
                if(typeMappings is null)
                    throw new InvalidOperationException("_publisherTypeMapping is no longer Dictionary<string, Type>");
                typeMappings[PublisherTargetType.SQS_PUBLISHER] = typeof(CustomSQSPublisher);
                
                return messagePublisher;
            }

            throw new InvalidOperationException("Something went wrong");
        });
        services.AddSingleton<ISQSMiddleware, TenantSQSMiddleware>();
        return services;
    }
}