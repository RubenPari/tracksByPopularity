namespace tracksByPopularity.Infrastructure.Configuration;

public class DatabaseSettings
{
    public string ConnectionString { get; set; } = "Server=localhost;Port=3306;Database=tracksbypopularity;User=root;Password=password;";
}
