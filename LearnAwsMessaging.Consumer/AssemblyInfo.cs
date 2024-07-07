using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;
using Amazon.Lambda.Serialization.SystemTextJson;
using System.Text.Json.Serialization;

[assembly: LambdaSerializer(typeof(SourceGeneratorLambdaJsonSerializer<CustomJsonSerializerContext>))]

[JsonSerializable(typeof(SQSEvent))]
[JsonSerializable(typeof(SQSBatchResponse))]
internal partial class CustomJsonSerializerContext : JsonSerializerContext;