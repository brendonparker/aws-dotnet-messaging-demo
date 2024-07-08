using AWS.Messaging;
using LearnAwsMessaging.Contracts;
using Microsoft.Extensions.Logging;

namespace LearnAwsMessaging.Consumer.Handlers.BackgroundJob;

public class DownloadReceiptsHandler(
    ILogger<DownloadReceiptsHandler> log,
    IMessagePublisher publisher) : IMessageHandler<DownloadReceipts>
{
    public async Task<MessageProcessStatus> HandleAsync(MessageEnvelope<DownloadReceipts> messageEnvelope,
        CancellationToken token = default)
    {
        log.LogInformation("Handling: {Type}", messageEnvelope.Message.GetType());

        // Simulating work to "download receipts from some external system"
        await Task.Delay(20, token);

        await publisher.PublishAsync(new ReceiptsDownloaded
        {
            TenantId = messageEnvelope.Message.TenantId,
            JobId = messageEnvelope.Message.JobId,
            Month = messageEnvelope.Message.Month
        }, token);

        return MessageProcessStatus.Success();
    }
}