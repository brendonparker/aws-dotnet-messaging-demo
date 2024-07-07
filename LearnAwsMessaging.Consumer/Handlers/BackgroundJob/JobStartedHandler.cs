using AWS.Messaging;
using LearnAwsMessaging.Contracts;
using Microsoft.Extensions.Logging;

namespace LearnAwsMessaging.Consumer.Handlers.BackgroundJob;

public class JobStartedHandler(
    ILogger<JobStartedHandler> log) : IMessageHandler<JobStarted>
{
    public Task<MessageProcessStatus> HandleAsync(MessageEnvelope<JobStarted> messageEnvelope,
        CancellationToken token = default)
    {
        log.LogInformation("Handling: {Type}", messageEnvelope.Message.GetType());

        return Task.FromResult(MessageProcessStatus.Success());
    }
}