using System.Text.Json.Serialization;
using Amazon.Lambda.Core;
using Amazon.Lambda.Serialization.SystemTextJson;
using Amazon.Lambda.SQSEvents;
using AWS.Messaging.Lambda;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LearnAwsMessaging.Consumer;

public class Function
{
    private readonly ILambdaMessaging _lambdaMessaging;

    public Function()
    {
        var serviceProvider = new ServiceCollection()
            .AddLogging(x => x.AddConsole())
            .AddAWSMessageBus(builder =>
            {
                builder.AddDemoMessages();
                builder.AddMessageHandlers(typeof(Function).Assembly);
                builder.AddLambdaMessageProcessor(options => { options.MaxNumberOfConcurrentMessages = 1; });
            })
            .AddAWSMessagingCustomizations()
            .BuildServiceProvider();
        _lambdaMessaging = serviceProvider.GetRequiredService<ILambdaMessaging>();
    }

    [LambdaSerializer(typeof(SourceGeneratorLambdaJsonSerializer<CustomJsonSerializerContext>))]
    public async Task<SQSBatchResponse> FunctionHandler(SQSEvent input, ILambdaContext context) =>
        await _lambdaMessaging.ProcessLambdaEventWithBatchResponseAsync(input, context);
}

[JsonSerializable(typeof(SQSEvent))]
[JsonSerializable(typeof(SQSBatchResponse))]
internal partial class CustomJsonSerializerContext : JsonSerializerContext;