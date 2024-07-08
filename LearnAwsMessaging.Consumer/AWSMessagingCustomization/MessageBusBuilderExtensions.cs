using AWS.Messaging.Configuration;
using AWS.Messaging.Publishers.SQS;
using LearnAwsMessaging.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace AWS.Messaging.Configuration;

public static class MessageBusBuilderExtensions
{
    private static readonly string ACCOUNT_ID = Environment.GetEnvironmentVariable("AWS_ACCOUNT_ID") ?? "";
    private static readonly string REGION_ID = Environment.GetEnvironmentVariable("AWS_REGION_ID") ?? "";

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

        public SQSPublisherHelper AddMiddleware<TMiddleware>() where TMiddleware : ISQSMiddleware
        {
            _builder.AddAdditionalService(new ServiceDescriptor(typeof(ISQSMiddleware), _queueUrl, typeof(TMiddleware),
                ServiceLifetime.Singleton));
            return this;
        }
    }
}