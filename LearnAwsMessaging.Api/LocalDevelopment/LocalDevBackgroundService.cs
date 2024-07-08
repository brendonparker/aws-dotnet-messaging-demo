using System.Threading.Channels;
using Amazon.Lambda.SQSEvents;
using Amazon.SimpleNotificationService.Model;
using Amazon.SQS.Model;
using AWS.Messaging.Lambda;

namespace LearnAwsMessaging.Api.LocalDevelopment;

public class LocalDevBackgroundService(IServiceProvider serviceProvider) : BackgroundService
{
    public static Channel<object> SqsChannel { get; } = Channel.CreateUnbounded<object>();

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var message = await SqsChannel.Reader.ReadAsync(stoppingToken);

            if (message is SendMessageRequest sqsMessage)
                await HandleAsync(sqsMessage);
            if (message is PublishRequest snsMessage)
                await HandleAsync(snsMessage);
        }
    }

    private async Task HandleAsync(SendMessageRequest message)
    {
        using var scope = serviceProvider.CreateScope();

        var messaging = scope.ServiceProvider.GetRequiredService<ILambdaMessaging>();
        var sqsEvent = new SQSEvent
        {
            Records = [ConvertToSQSEventMessage(message)]
        };

        await messaging.ProcessLambdaEventAsync(sqsEvent, new LocalDevLambdaContext());
    }
    
    private async Task HandleAsync(PublishRequest message)
    {
        using var scope = serviceProvider.CreateScope();

        var messaging = scope.ServiceProvider.GetRequiredService<ILambdaMessaging>();
        var sqsEvent = new SQSEvent
        {
            Records = [ConvertToSQSEventMessage(message)]
        };

        await messaging.ProcessLambdaEventAsync(sqsEvent, new LocalDevLambdaContext());
    }

    private static SQSEvent.SQSMessage ConvertToSQSEventMessage(SendMessageRequest message) =>
        new()
        {
            MessageId = null,
            ReceiptHandle = null,
            Body = message.MessageBody,
            Md5OfBody = null,
            Md5OfMessageAttributes = null,
            EventSourceArn = "arn:aws:sns:us-east-1:0123456:localdev",
            EventSource = "",
            AwsRegion = "us-east-1",
            Attributes = new(),
            MessageAttributes = message.MessageAttributes
                .ToDictionary(x => x.Key, x => new SQSEvent.MessageAttribute
                {
                    StringValue = x.Value.StringValue,
                    BinaryValue = x.Value.BinaryValue,
                    StringListValues = x.Value.StringListValues,
                    BinaryListValues = x.Value.BinaryListValues,
                    DataType = x.Value.DataType
                })
        };
    
    private static SQSEvent.SQSMessage ConvertToSQSEventMessage(PublishRequest message) =>
        new()
        {
            MessageId = null,
            ReceiptHandle = null,
            Body = message.Message,
            Md5OfBody = null,
            Md5OfMessageAttributes = null,
            EventSourceArn = "arn:aws:sns:us-east-1:0123456:localdev",
            EventSource = "",
            AwsRegion = "us-east-1",
            Attributes = new(),
            MessageAttributes = message.MessageAttributes
                .ToDictionary(x => x.Key, x => new SQSEvent.MessageAttribute
                {
                    StringValue = x.Value.StringValue,
                    BinaryValue = x.Value.BinaryValue,
                    StringListValues = [],
                    BinaryListValues = [],
                    DataType = x.Value.DataType
                })
        };
}