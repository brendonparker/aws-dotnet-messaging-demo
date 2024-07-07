dotnet publish -c Release ./LearnAwsMessaging.Api -o ./publish/LearnAwsMessaging.Api
dotnet publish -c Release ./LearnAwsMessaging.Consumer -o ./publish/LearnAwsMessaging.Consumer
cdk deploy