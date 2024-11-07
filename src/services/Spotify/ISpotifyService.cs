namespace tracksByPopularity.services.Spotify;

public interface ISpotifyService
{
    Task<string> GetAccessTokenAsync();
}