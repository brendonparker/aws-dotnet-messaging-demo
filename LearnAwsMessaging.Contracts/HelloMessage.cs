namespace LearnAwsMessaging.Contracts;

public sealed record HelloMessage : ITenant
{
    public required string TenantId { get; init; }
    public required string Name { get; init; }
}
