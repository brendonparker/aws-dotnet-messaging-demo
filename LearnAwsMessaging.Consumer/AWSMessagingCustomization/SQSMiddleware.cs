namespace AWS.Messaging.Publishers.SQS;

public interface ISQSMiddleware
{
    Task<SQSOptions?> HandleAsync<T>(string queueUrl, T message, SQSOptions? options);
}