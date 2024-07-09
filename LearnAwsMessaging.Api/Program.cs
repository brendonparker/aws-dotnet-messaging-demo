using AWS.Messaging;
using LearnAwsMessaging.Api;
using LearnAwsMessaging.Consumer;
using LearnAwsMessaging.Contracts;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAWSLambdaHosting(LambdaEventSource.HttpApi);
builder.Services.AddAWSMessageBus(messageBus =>
{
    messageBus.AddDemoMessages();
    if (builder.Environment.IsDevelopment())
    {
        messageBus.AddMessageHandlers(typeof(Function).Assembly);
        messageBus.AddLambdaMessageProcessor(options =>
        {
            options.MaxNumberOfConcurrentMessages = 1;
        });
    }
});
builder.Services.AddAWSMessagingCustomizations();
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddLocalDevelopmentServices();
}
var app = builder.Build();

app.MapGet("/", async ([FromServices] IMessagePublisher publisher, [FromQuery] string? tenantId = null) =>
{
    await publisher.PublishAsync(new HelloMessage { Name = "World!", TenantId = tenantId ?? "Acme" });
    return "Message Published. Try /startjob ";
});

app.MapGet("/startjob", async ([FromServices] IMessagePublisher publisher, [FromQuery] string? tenantId = null) =>
{
    await publisher.PublishAsync(new StartJob { TenantId = "Acme", JobId = Guid.NewGuid().ToString() });
    return "Message Published";
});

app.Run();