namespace tracksByPopularity.services;

public interface IAuthService
{
    Task<string> GetAccessTokenAsync();

    void Logout();
}