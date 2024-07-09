namespace AWS.Messaging.Publishers.SQS;

public interface ISQSMiddleware
{
    Task<SQSOptions?> HandleAsync<T>(T message, SQSOptions? options);
}