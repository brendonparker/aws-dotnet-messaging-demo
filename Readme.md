
## Goals

1. Learn and evaluate the [aws-dotnet-messaging](https://github.com/awslabs/aws-dotnet-messaging) library
2. Build a demo that can run serverlessly.

   2a. Use dotnet + Lambda for a web API.
   
   2b. Use a lambda to process SQS events. No long-running compute for polling (that I own).
3. Look for possible ways of improving it.

## Background

I have already written some functionality in an existing system that leverage AWS Step Functions. Works well, but I don't like to be so closely coupled to that service. Additionally I needed to introduce some queueing into the process to keep certain steps from running concurrently.

I initially was looking to use MassTransit, and leverage their Saga patterns for dealing with state for long-running processes. After spending about a week trying to get MassTransit to work, I gave up. The documentation was lacking and MT can do way more than I needed it for, which seemed to complicate the experience (possible "skill issue" 😄 ). There also seemed to be a lot of discouragement against putting everything into one (or two) queues. Combine that with the way their SQS + Lambda sample worked, it was a no-go.

I hope to iterate on this demo to add some other form of state machines. But for now, I'm just proving out the commuincation patterns using aws-dotnet-messaging.

## Description of problem domain

Admittedly, the scenario I'm using may not be the best. But here it goes...

I want a multi-tenant API that can kick off a background job. This background job consists of multiple steps. Download sales order data and receipt data from a third party system. Possibly loop by month. Any communication communication/events around downloading data from the third party/external service need to use a FIFO topic/queue as this service has strict concurrency limits. I can leverage `MessageGroupId` of the FIFO queue to put each tenants messages in their own "group".


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

