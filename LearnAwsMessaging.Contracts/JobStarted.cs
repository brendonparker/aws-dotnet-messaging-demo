namespace LearnAwsMessaging.Contracts;

public sealed record JobStarted
{
    public required string JobId { get; init; }
}