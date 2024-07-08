using AWS.Messaging;
using LearnAwsMessaging.Contracts;
using Microsoft.Extensions.Logging;

namespace LearnAwsMessaging.Consumer.Handlers.BackgroundJob;

public class StartJobHandler(
    ILogger<StartJobHandler> log,
    IMessagePublisher publisher) : IMessageHandler<StartJob>
{
    public async Task<MessageProcessStatus> HandleAsync(MessageEnvelope<StartJob> messageEnvelope,
        CancellationToken token = default)
    {
        log.LogInformation("Handling: {Type}", messageEnvelope.Message.GetType());
        await publisher.PublishAsync(new DownloadReceipts
        {
            TenantId = messageEnvelope.Message.TenantId,
            JobId = messageEnvelope.Message.JobId,
            // TODO: Make this make more sense for demo: 
            Month = new DateOnly(DateTime.Now.Year, DateTime.Now.Month, 1).AddMonths(-3)
        }, token);

        await publisher.PublishAsync(new JobStarted
        {
            JobId = messageEnvelope.Message.JobId
        }, token);

        return MessageProcessStatus.Success();
    }
}