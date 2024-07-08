using AWS.Messaging.Publishers.SQS;

namespace LearnAwsMessaging.Consumer.AWSMessagingCustomization;

public interface ISQSMiddlewareProvider
{
    IEnumerable<ISQSMiddleware> Resolve(string queueUrl);
}