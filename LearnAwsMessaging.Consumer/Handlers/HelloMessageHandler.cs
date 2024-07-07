using AWS.Messaging;
using LearnAwsMessaging.Contracts;
using Microsoft.Extensions.Logging;

namespace LearnAwsMessaging.Consumer.Handlers;

public class HelloMessageHandler(ILogger<HelloMessageHandler> log) :
    IMessageHandler<HelloMessage>
{
    public Task<MessageProcessStatus> HandleAsync(MessageEnvelope<HelloMessage> messageEnvelope,
        CancellationToken token = default)
    {
        if (messageEnvelope.Message.TenantId == "ERROR")
            throw new InvalidOperationException("ERROR tenant encountered");

        log.LogInformation("{SourceAddress} Hello {Name}", messageEnvelope.Source,
            messageEnvelope.Message.Name);

        return Task.FromResult(MessageProcessStatus.Success());
    }
}