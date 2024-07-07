using AWS.Messaging;
using LearnAwsMessaging.Contracts;
using Microsoft.Extensions.Logging;

namespace LearnAwsMessaging.Consumer.Handlers.BackgroundJob;

public class ReceiptsDownloadedHandler(
    ILogger<ReceiptsDownloadedHandler> log,
    IMessagePublisher publisher) : IMessageHandler<ReceiptsDownloaded>
{
    public async Task<MessageProcessStatus> HandleAsync(MessageEnvelope<ReceiptsDownloaded> messageEnvelope,
        CancellationToken token = default)
    {
        log.LogInformation("Handling: {Type}", messageEnvelope.Message.GetType());
        // Simulating work to "download receipts from some external system"
        await Task.Delay(20);

        await publisher.PublishAsync(new DownloadSales
        {
            JobId = messageEnvelope.Message.JobId,
            Month = messageEnvelope.Message.Month,
            TenantId = messageEnvelope.Message.TenantId
        }, token);

        return MessageProcessStatus.Success();
    }
}