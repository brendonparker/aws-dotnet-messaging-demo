using System.Reflection;

namespace AWS.Messaging.Telemetry;

/// <summary>
/// Constants related to telemetry
/// </summary>
public static class MyTelemetryKeys
{
    /// <summary>
    /// Current version of the AWS.Messaging package
    /// </summary>
    public static string AWSMessagingAssemblyVersion =
        typeof(IMessagePublisher).Assembly.GetName().Version?.ToString() ?? string.Empty;

    internal const string QueueUrl = "aws.messaging.sqs.queueurl";
    internal const string SqsMessageId = "aws.messaging.sqs.messageId";
    internal const string TopicUrl = "aws.messaging.sns.topicUrl";
    internal const string EventBusName = "aws.messaging.eventBridge.eventBusName";
    internal const string ObjectType = "aws.messaging.objectType";
    internal const string MessageType = "aws.messaging.messageType";
    internal const string MessageId = "aws.messaging.messageId";
    internal const string PublishTargetType = "aws.messaging.publishTargetType";
    internal const string HandlerType = "aws.messaging.handlerType";
}