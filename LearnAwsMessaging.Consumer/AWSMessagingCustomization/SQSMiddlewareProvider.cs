using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;

namespace AWS.Messaging.Publishers.SQS;

public class SQSMiddlewareProvider(IServiceProvider serviceProvider) : ISQSMiddlewareProvider
{
    private readonly ConcurrentDictionary<string, IEnumerable<ISQSMiddleware>> _middlewares = new();

    public IEnumerable<ISQSMiddleware> Resolve(string queueUrl) =>
        _middlewares.GetOrAdd(queueUrl, (key) =>
            serviceProvider.GetKeyedService<IEnumerable<ISQSMiddleware>>(key) ?? []
        );
}