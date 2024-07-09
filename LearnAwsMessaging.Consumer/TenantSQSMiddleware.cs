using AWS.Messaging.Publishers.SQS;
using LearnAwsMessaging.Contracts;

namespace LearnAwsMessaging.Consumer;

public class TenantSQSMiddleware : ISQSMiddleware
{
    public Task<SQSOptions?> HandleAsync<T>(T message, SQSOptions? options)
    {
        if (message is ITenant tenant)
        {
            options ??= new();
            options.MessageGroupId = tenant.TenantId;
        }

        return Task.FromResult(options);
    }
}