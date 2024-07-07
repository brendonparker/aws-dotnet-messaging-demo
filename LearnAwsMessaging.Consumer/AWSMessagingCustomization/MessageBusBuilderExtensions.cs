using AWS.Messaging.Configuration;
using LearnAwsMessaging.Contracts;

namespace AWS.Messaging.Configuration;

public static class MessageBusBuilderExtensions
{
    const string ACCOUNT_ID = "0123456789";
    const string REGION_ID = "us-east-1";

    public static SNSPublisherHelper AddSnsTopic(this MessageBusBuilder builder, string topicNameOrArn) =>
        new(builder, topicNameOrArn);

    public static SQSPublisherHelper AddSqsQueue(this MessageBusBuilder builder, string queueNameOrUrl) =>
        new(builder, queueNameOrUrl);

    public class SNSPublisherHelper
    {
        private readonly MessageBusBuilder _builder;
        private readonly string _topicArn;

        public SNSPublisherHelper(MessageBusBuilder builder, string topicNameOrArn)
        {
            _builder = builder;
            _topicArn = topicNameOrArn.StartsWith("arn:aws:sns")
                ? topicNameOrArn
                : $"arn:aws:sns:{REGION_ID}:{ACCOUNT_ID}:{topicNameOrArn}";
        }

        public SNSPublisherHelper RouteMessageType<T>()
        {
            _builder.AddSNSPublisher<T>(_topicArn);
            return this;
        }
    }

    public class SQSPublisherHelper
    {
        private readonly MessageBusBuilder _builder;
        private readonly string _queueUrl;

        public SQSPublisherHelper(MessageBusBuilder builder, string queueNameOrUrl)
        {
            _builder = builder;
            _queueUrl = queueNameOrUrl.StartsWith("https://sqs")
                ? queueNameOrUrl
                : $"https://sqs.{REGION_ID}.amazonaws.com/{ACCOUNT_ID}/{queueNameOrUrl}";
        }

        public SQSPublisherHelper RouteMessageType<T>()
        {
            _builder.AddSQSPublisher<T>(_queueUrl);
            return this;
        }
    }
}