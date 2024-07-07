using AWS.Messaging;
using LearnAwsMessaging.Contracts;
using Microsoft.Extensions.Logging;

namespace LearnAwsMessaging.Consumer.Handlers.BackgroundJob;

public class SalesDownloadedHandler(ILogger<SalesDownloadedHandler> log) : IMessageHandler<SalesDownloaded>
{
    public async Task<MessageProcessStatus> HandleAsync(MessageEnvelope<SalesDownloaded> messageEnvelope,
        CancellationToken token = default)
    {
        log.LogInformation("Handling: {Type}", messageEnvelope.Message.GetType());
        await Task.CompletedTask;
        return MessageProcessStatus.Success();
    }
}