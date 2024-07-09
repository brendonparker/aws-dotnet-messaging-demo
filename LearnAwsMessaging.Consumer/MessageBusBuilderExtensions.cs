using System.Reflection;
using AWS.Messaging;
using AWS.Messaging.Configuration;
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

    public static MessageBusBuilder AddMessageHandlers(this MessageBusBuilder builder, Assembly assembly)
    {
        var handlerInterfaceType = typeof(IMessageHandler<>);

        var types = assembly.GetTypes()
            .Where(type => type is { IsAbstract: false, IsInterface: false })
            .Select(type => new
            {
                ImplementationType = type,
                InterfaceTypes = type.GetInterfaces()
                    .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == handlerInterfaceType)
                    .ToList()
            })
            .Where(t => t.InterfaceTypes.Count != 0)
            .ToList();

        var methodInfo = typeof(MessageBusBuilder).GetMethod(nameof(MessageBusBuilder.AddMessageHandler));
        foreach (var type in types)
        {
            foreach (var interfaceType in type.InterfaceTypes)
            {
                var genericMethod =
                    methodInfo?.MakeGenericMethod([type.ImplementationType, interfaceType.GetGenericArguments()[0]]);
                genericMethod?.Invoke(builder, [null]);
            }
        }

        return builder;
    }
}