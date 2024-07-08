using AWS.Messaging.Configuration;
using LearnAwsMessaging.Consumer.Handlers;
using LearnAwsMessaging.Consumer.Handlers.BackgroundJob;
using LearnAwsMessaging.Contracts;

namespace LearnAwsMessaging.Consumer;

public static class MessageBusBuilderExtensions
{
    public static MessageBusBuilder AddDemoMessages(this MessageBusBuilder builder)
    {
        // Standard messages
        builder.AddSqsQueue("aws-msg-demo")
            .RouteMessageType<HelloMessage>()
            .RouteMessageType<StartJob>();

        // FIFO messages
        builder.AddSqsQueue("aws-msg-demo.fifo")
            .RouteMessageType<DownloadSales>()
            .RouteMessageType<DownloadReceipts>()
            .AddMiddleware<TenantSQSMiddleware>();

        // SNS Notifications
        builder.AddSnsTopic("aws-msg-demo-job-started").RouteMessageType<JobStarted>();
        builder.AddSnsTopic("aws-msg-demo-sales-downloaded").RouteMessageType<SalesDownloaded>();
        builder.AddSnsTopic("aws-msg-demo-receipts-downloaded").RouteMessageType<ReceiptsDownloaded>();

        return builder;
    }

    public static MessageBusBuilder AddDemoMessageHandlers(this MessageBusBuilder builder)
    {
        builder.AddMessageHandler<HelloMessageHandler, HelloMessage>();
        builder.AddMessageHandler<StartJobHandler, StartJob>();
        builder.AddMessageHandler<JobStartedHandler, JobStarted>();
        builder.AddMessageHandler<DownloadReceiptsHandler, DownloadReceipts>();
        builder.AddMessageHandler<ReceiptsDownloadedHandler, ReceiptsDownloaded>();
        builder.AddMessageHandler<SalesDownloadedHandler, SalesDownloaded>();
        builder.AddMessageHandler<DownloadSalesHandler, DownloadSales>();

        return builder;
    }
}