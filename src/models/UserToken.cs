namespace tracksByPopularity.models;

public class UserToken
{
    public required string UserId { get; set; }
    public required string AccessToken { get; set; }
    public required DateTime ExpiresAt { get; set; }
}
