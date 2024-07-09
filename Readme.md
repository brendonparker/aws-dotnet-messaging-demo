## Goals

1. Learn and evaluate the [aws-dotnet-messaging](https://github.com/awslabs/aws-dotnet-messaging) library
2. Build a demo that can run serverlessly.

   2a. Use dotnet + Lambda for a web API.

   2b. Use a lambda to process SQS events. No long-running compute for polling (that I own).
3. Look for possible ways of improving it.

## Background

I have already written some functionality in an existing system that leverage AWS Step Functions. Works well, but I
don't like to be so closely coupled to that service. Additionally I needed to introduce some queueing into the process
to keep certain steps from running concurrently.

I initially was looking to use MassTransit, and leverage their Saga patterns for dealing with state for long-running
processes. After spending about a week trying to get MassTransit to work, I gave up. The documentation was lacking and
MT can do way more than I needed it for, which seemed to complicate the experience (possible "skill issue" 😄 ). There
also seemed to be a lot of discouragement against putting everything into one (or two) queues. Combine that with the way
their SQS + Lambda sample worked, it was a no-go.

I hope to iterate on this demo to add some other form of state machines. But for now, I'm just proving out the
commuincation patterns using aws-dotnet-messaging.

## Description of problem domain

Admittedly, the scenario I'm using may not be the best. But here it goes...

I want a multi-tenant API that can kick off a background job. This background job consists of multiple steps. Download
sales order data and receipt data from a third party system. Possibly loop by month. Any communication
communication/events around downloading data from the third party/external service need to use a FIFO topic/queue as
this service has strict concurrency limits. I can leverage `MessageGroupId` of the FIFO queue to put each tenants
messages in their own "group".

## "Improvements" made to the out-of-the-box AWS Messaging library

Perhaps I should have forked the library and used the fork. But I think what I have will serve to demonstrate some of
the changes that I think could improve the library. But beware: my approach is very hacky (using reflection to change
private members in places).

### The ability to configure a queue or topic by name without needing the full URL.

This assumes that the runtime can identify the current AWS account and region and therefore can construct the proper
url/arn.
For now, I've passed some environment variables through from the CDK to the Lambda, which the code can then pickup
and use. TBD if there is a better way to do this (without adding API calls).

The net effect is something that looks like:

```csharp
// Standard messages
builder.AddSqsQueue("aws-msg-demo")
    .RouteMessageType<HelloMessage>()
    .RouteMessageType<StartJob>();

// FIFO messages
builder.AddSqsQueue("aws-msg-demo.fifo")
    .RouteMessageType<DownloadSales>()
    .RouteMessageType<DownloadReceipts>();

// SNS Notifications
builder.AddSnsTopic("aws-msg-demo-job-started").RouteMessageType<JobStarted>();
builder.AddSnsTopic("aws-msg-demo-sales-downloaded").RouteMessageType<SalesDownloaded>();
builder.AddSnsTopic("aws-msg-demo-receipts-downloaded").RouteMessageType<ReceiptsDownloaded>();
```

which is less verbose than doing:

```csharp
// Standard messages
builder.AddSQSPublisher<HelloMessage>("https://sqs.us-east-1.amazonaws.com/0123456789/aws-msg-demo");
builder.AddSQSPublisher<StartJob>("https://sqs.us-east-1.amazonaws.com/0123456789/aws-msg-demo");
  
// FIFO messages
builder.AddSQSPublisher<DownloadSales>("https://sqs.us-east-1.amazonaws.com/0123456789/aws-msg-demo.fifo");
builder.AddSQSPublisher<DownloadReceipts>("https://sqs.us-east-1.amazonaws.com/0123456789/aws-msg-demo.fifo");
  
// SNS Notifications
builder.AddSNSPublisher<JobStarted>("arn:aws:sns:us-east-1:0123456789:aws-msg-demo-job-started");
builder.AddSNSPublisher<SalesDownloaded>("arn:aws:sns:us-east-1:0123456789:aws-msg-demo-sales-downloaded");
builder.AddSNSPublisher<ReceiptsDownloaded>("arn:aws:sns:us-east-1:0123456789:aws-msg-demo-receipts-downloaded");
```

### The ability to add middleware to run when publishing messages

This is most useful in my use cases when using a FIFO queue, and wanting to uniformly use `IMessagePublisher` rather
than
having to use `ISQSPublisher` in order to be able to specify the `MessageGroupId` on the `SQSOptions`. Which you have to
have tribal knowledge in order to know when to use which publisher.

There were two ways I could see this working. 1) where the middleware receives the queueUrl. So that its logic can be
specific to a queue (i.e. only apply MessageGroupId to things bound to a FIFO queue). 2) The middleware is keyed to a
specific queue, so the middleware becomes queue specific. I went with approach #2, but that may prove to be more
restrictive. For example: what if you want middleware that applies to all queues?

To make this happen, I had to do some naughty things. I needed to replace the implementation of `ISQSPublisher` which is
controlled via a private field/mapping within the internal class: `MessageRoutingPublisher`. So I used reflection to
manipulate `_publisherTypeMapping`, replacing `SQSPublisher` with my custom `CustomSQSPublisher`. My implementation is a
copy/paste of the original, but with the middleware added in.

I can then leverage my enhancements from above to make the middleware queue-specific in a fairly fluent syntax:

```csharp
// FIFO messages
builder.AddSqsQueue("aws-msg-demo.fifo")
   .RouteMessageType<DownloadSales>()
   .RouteMessageType<DownloadReceipts>()
   .AddMiddleware<TenantSQSMiddleware>();
```

## Bonus: Local Development

What if I want to develop locally without needing to setup something like localstack or some other heavy third party
dependency?
How do I test my message handlers in my local dev environment if the queues are being consumed by lambda integrations?

My solution to this was to create a background service (`LocalDevBackgroundService`) which only kicks in when running in
debug/Development mode. Additionally, when in this mode, I sub in my own `IAmazonSQS`
and `IAmazonSimpleNotificationService` implementations which push things into this `LocalDevBackgroundService` for
processing. The `LocalDevBackgroundService` can use the already existing `ILambdaMessaging` implementation to process the
messages, just like if it were a lambda processing SQS messages.

This approach works fine in this demo, but is incomplete as it assumes all sns topics and sqs messages are going to be
processed by the same consumer. More work would need to be done to give this more intelligence. The AWS.Messaging
library currently relies on the plumbing set outside of this code to know how to route SNS messages to SQS queues, for
example.

## Deploy Instructions

To deploy, you must first install the aws-cdk cli: `npm install -g aws-cdk@2.147.0`

You'll need to install the npm packages for the IAC (infrastructure as code) project:

```shell
cd IAC
npm install
```

From the root directory, run the following commands to build the required artifacts:

```shell
dotnet publish -c Release ./LearnAwsMessaging.Api -o ./publish/LearnAwsMessaging.Api
dotnet publish -c Release ./LearnAwsMessaging.Consumer -o ./publish/LearnAwsMessaging.Consumer
```

Then can deploy using: `cdk deploy`

> [!NOTE]
> Using `cdk watch` can be very helpful when trying to just deploy the artifacts and tailing the logs

