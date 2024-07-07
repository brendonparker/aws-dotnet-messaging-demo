using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using AWS.Messaging.Lambda;
using AWS.Messaging.Publishers.SQS;
using LearnAwsMessaging.Consumer.Handlers;
using LearnAwsMessaging.Consumer.Handlers.BackgroundJob;
using LearnAwsMessaging.Contracts;

namespace LearnAwsMessaging.Consumer;

public class Function
{
    private readonly ServiceProvider _serviceProvider;

    public Function()
    {
        _serviceProvider = new ServiceCollection()
            .AddLogging(x => x.AddConsole())
            .AddAWSMessageBus(builder =>
            {
                builder.AddDemoMessages();
                builder.AddMessageHandler<HelloMessageHandler, HelloMessage>();
                builder.AddMessageHandler<StartJobHandler, StartJob>();
                builder.AddMessageHandler<JobStartedHandler, JobStarted>();
                builder.AddMessageHandler<DownloadReceiptsHandler, DownloadReceipts>();
                builder.AddMessageHandler<ReceiptsDownloadedHandler, ReceiptsDownloaded>();
                builder.AddMessageHandler<SalesDownloadedHandler, SalesDownloaded>();
                builder.AddMessageHandler<DownloadSalesHandler, DownloadSales>();
                builder.AddLambdaMessageProcessor(options =>
                {
                    options.MaxNumberOfConcurrentMessages = 1;
                });
            })
            .AddAWSMessagingCustomizations()
            .AddSingleton<ISQSMiddleware, TenantSQSMiddleware>()
            .BuildServiceProvider();
    }

    public async Task<SQSBatchResponse> FunctionHandler(SQSEvent input, ILambdaContext context)
    {
        using var scope = _serviceProvider.CreateScope();

        var messaging = scope.ServiceProvider.GetRequiredService<ILambdaMessaging>();

        return await messaging.ProcessLambdaEventWithBatchResponseAsync(input, context);
    }
}