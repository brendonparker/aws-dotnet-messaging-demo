using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using AWS.Messaging.Lambda;

namespace LearnAwsMessaging.Consumer;

public class Function
{
    private readonly ServiceProvider _serviceProvider;

    public Function()
    {
        _serviceProvider = new ServiceCollection()
            .AddLogging(x => x.AddConsole())
            .AddAWSMessageBus(builder =>
            {
                builder.AddDemoMessages();
                builder.AddDemoMessageHandlers();
                builder.AddLambdaMessageProcessor(options =>
                {
                    options.MaxNumberOfConcurrentMessages = 1;
                });
            })
            .AddAWSMessagingCustomizations()
            .BuildServiceProvider();
    }

    public async Task<SQSBatchResponse> FunctionHandler(SQSEvent input, ILambdaContext context)
    {
        using var scope = _serviceProvider.CreateScope();

        var messaging = scope.ServiceProvider.GetRequiredService<ILambdaMessaging>();

        return await messaging.ProcessLambdaEventWithBatchResponseAsync(input, context);
    }
}