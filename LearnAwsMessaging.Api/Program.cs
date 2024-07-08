using AWS.Messaging;
using LearnAwsMessaging.Consumer;
using LearnAwsMessaging.Contracts;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAWSLambdaHosting(LambdaEventSource.HttpApi);
builder.Services.AddAWSMessageBus(builder => { builder.AddDemoMessages(); });
builder.Services.AddAWSMessagingCustomizations();
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