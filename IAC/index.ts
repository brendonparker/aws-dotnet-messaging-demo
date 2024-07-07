#!/usr/bin/env node
import "source-map-support/register";
import * as cdk from "aws-cdk-lib";
import { AwsMessagingSampleStackSampleStack } from "./lib/AwsMessagingSampleStack";

const app = new cdk.App();
new AwsMessagingSampleStackSampleStack(app, "AwsMessagingSampleStackSampleStack", {
  env: {
    account: process.env.CDK_DEFAULT_ACCOUNT,
    region: process.env.CDK_DEFAULT_REGION,
  },
  tags: {
    CreatedBy: "Brendon",
    Application: "aws-msg-demo",
  },
});
