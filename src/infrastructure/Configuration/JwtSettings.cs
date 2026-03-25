namespace tracksByPopularity.Infrastructure.Configuration;

public class JwtSettings
{
    public string Secret { get; set; } = "default-secret-key-change-in-production-min-32-chars";
    public string Issuer { get; set; } = "tracksByPopularity";
    public string Audience { get; set; } = "tracksByPopularity";
    public int ExpiryDays { get; set; } = 7;
}
