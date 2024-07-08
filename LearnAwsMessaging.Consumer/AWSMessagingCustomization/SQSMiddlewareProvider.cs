using System.Collections.Concurrent;
using AWS.Messaging.Publishers.SQS;
using Microsoft.Extensions.DependencyInjection;

namespace LearnAwsMessaging.Consumer.AWSMessagingCustomization;

public class SQSMiddlewareProvider(IServiceProvider serviceProvider) : ISQSMiddlewareProvider
{
    private readonly ConcurrentDictionary<string, IEnumerable<ISQSMiddleware>> _middlewares = new();

    public IEnumerable<ISQSMiddleware> Resolve(string queueUrl) =>
        _middlewares.GetOrAdd(queueUrl, (key) =>
            serviceProvider.GetKeyedService<IEnumerable<ISQSMiddleware>>(key) ?? []
        );
}