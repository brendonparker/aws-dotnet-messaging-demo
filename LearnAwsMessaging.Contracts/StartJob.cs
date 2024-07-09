namespace LearnAwsMessaging.Contracts;

public sealed record StartJob : ITenant
{
    public required string TenantId { get; init; }
    public required string JobId { get; init; }
}