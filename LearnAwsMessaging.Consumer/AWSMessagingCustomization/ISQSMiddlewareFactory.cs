using AWS.Messaging.Publishers.SQS;

namespace AWS.Messaging.Publishers.SQS;

public interface ISQSMiddlewareProvider
{
    IEnumerable<ISQSMiddleware> Resolve(string queueUrl);
}