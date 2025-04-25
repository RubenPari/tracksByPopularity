namespace tracksByPopularity.models;

public class ApiKey
{
    public required string Key { get; init; }
    public required string UserId { get; init; }
    public required string Description { get; init; }
    public required DateTime CreatedAt { get; init; }
    public required DateTime ExpiresAt { get; init; }
    public bool IsActive { get; set; } = true;
}
