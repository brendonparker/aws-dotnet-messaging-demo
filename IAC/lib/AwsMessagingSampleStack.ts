import * as cdk from "aws-cdk-lib";
import { Construct } from "constructs";
import * as lmbda from "aws-cdk-lib/aws-lambda";
import * as es from "aws-cdk-lib/aws-lambda-event-sources";
import * as sqs from "aws-cdk-lib/aws-sqs";
import * as sub from "aws-cdk-lib/aws-sns-subscriptions";
import * as sns from "aws-cdk-lib/aws-sns";
import * as iam from "aws-cdk-lib/aws-iam";

export class AwsMessagingSampleStackSampleStack extends cdk.Stack {
  topics: sns.Topic[];
  constructor(scope: Construct, id: string, props?: cdk.StackProps) {
    super(scope, id, props);

    this.topics = [];
    const prefix = "aws-msg-demo";

    this.addTopic(
      new sns.Topic(this, "TopicJobStarted", {
        topicName: `${prefix}-job-started`,
      })
    );

    this.addTopic(
      new sns.Topic(this, "TopicMonthTransitioned", {
        topicName: `${prefix}-month-transitioned`,
      })
    );

    this.addTopic(
      new sns.Topic(this, "TopicSalesDownloaded", {
        topicName: `${prefix}-sales-downloaded`,
      })
    );

    this.addTopic(
      new sns.Topic(this, "TopicReceiptsDownloaded", {
        topicName: `${prefix}-receipts-downloaded`,
      })
    );

    const deadLetterQueueFifo = new sqs.Queue(this, "DLQFifo", {
      contentBasedDeduplication: true,
      queueName: `${prefix}-dlq.fifo`,
      visibilityTimeout: cdk.Duration.seconds(5),
      fifoThroughputLimit: sqs.FifoThroughputLimit.PER_MESSAGE_GROUP_ID,
      deduplicationScope: sqs.DeduplicationScope.MESSAGE_GROUP,
    });

    const queueFifo = new sqs.Queue(this, "QueueFifo", {
      contentBasedDeduplication: true,
      queueName: `${prefix}.fifo`,
      visibilityTimeout: cdk.Duration.minutes(5),
      fifoThroughputLimit: sqs.FifoThroughputLimit.PER_MESSAGE_GROUP_ID,
      deduplicationScope: sqs.DeduplicationScope.MESSAGE_GROUP,
      deadLetterQueue: {
        queue: deadLetterQueueFifo,
        maxReceiveCount: 1,
      },
    });

    const deadLetterQueue = new sqs.Queue(this, "DLQ", {
      queueName: `${prefix}-dlq`,
      visibilityTimeout: cdk.Duration.minutes(5),
    });

    const queue = new sqs.Queue(this, "Queue", {
      queueName: `${prefix}`,
      visibilityTimeout: cdk.Duration.minutes(5),
      deadLetterQueue: {
        queue: deadLetterQueue,
        maxReceiveCount: 1,
      },
    });

    const lambdaConsumer = new lmbda.Function(this, "LambdaConsumer", {
      code: lmbda.Code.fromAsset("./publish/LearnAwsMessaging.Consumer"),
      runtime: lmbda.Runtime.DOTNET_8,
      memorySize: 1536,
      architecture: lmbda.Architecture.ARM_64,
      functionName: `${prefix}-consumer`,
      timeout: cdk.Duration.minutes(3),
      handler:
        "LearnAwsMessaging.Consumer::LearnAwsMessaging.Consumer.Function::FunctionHandler",
    });

    const lambdaApi = new lmbda.Function(this, "Lambda", {
      code: lmbda.Code.fromAsset("./publish/LearnAwsMessaging.Api"),
      runtime: lmbda.Runtime.DOTNET_8,
      memorySize: 1536,
      architecture: lmbda.Architecture.ARM_64,
      handler: "LearnAwsMessaging.Api",
      functionName: `${prefix}-api`,
      timeout: cdk.Duration.seconds(30),
    });

    lambdaApi.role?.addToPrincipalPolicy(
      new iam.PolicyStatement({
        resources: ["*"],
        actions: ["SNS:ListTopics", "SNS:Publish"],
      })
    );

    const lambdaUrl = lambdaApi.addFunctionUrl({
      authType: lmbda.FunctionUrlAuthType.NONE,
    });

    queue.grantSendMessages(lambdaApi);
    queueFifo.grantSendMessages(lambdaApi);

    queue.grantSendMessages(lambdaConsumer);
    queueFifo.grantSendMessages(lambdaConsumer);

    lambdaConsumer.addEventSource(
      new es.SqsEventSource(queue, {
        batchSize: 2,
        reportBatchItemFailures: true,
      })
    );

    lambdaConsumer.addEventSource(
      new es.SqsEventSource(queueFifo, {
        batchSize: 2,
        reportBatchItemFailures: true,
      })
    );

    for (const topic of this.topics) {
      topic.addSubscription(new sub.SqsSubscription(queue));
      topic.grantPublish(lambdaApi);
      topic.grantPublish(lambdaConsumer);
    }

    new cdk.CfnOutput(this, "FunctionUrl", {
      value: lambdaUrl.url,
    });
  }

  addTopic(topic: sns.Topic) {
    this.topics.push(topic);
    return topic;
  }
}
