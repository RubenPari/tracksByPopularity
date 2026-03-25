namespace tracksByPopularity.Infrastructure.Configuration;

public class MailtrapSettings
{
    public string ApiKey { get; set; } = string.Empty;
    public string FromEmail { get; set; } = "noreply@trackbypopularity.com";
    public string FromName { get; set; } = "Tracks by Popularity";
    public string ClientUrl { get; set; } = "http://localhost:5173";
}
