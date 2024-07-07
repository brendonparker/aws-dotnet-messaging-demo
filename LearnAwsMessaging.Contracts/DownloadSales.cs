namespace LearnAwsMessaging.Contracts;

public sealed class DownloadSales : ITenant
{
    public required string TenantId { get; init; }
    public required string JobId { get; init; }
    public required DateOnly Month { get; init; }
}