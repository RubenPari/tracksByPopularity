using SpotifyAPI.Web;
using tracksByPopularity.models;
using tracksByPopularity.utils;

namespace tracksByPopularity.controllers;

public abstract class AuthController(ITokenService tokenService)
{
    public static IResult Login()
    {
        var request = new LoginRequest(
            new Uri(Constants.RedirectUri),
            Constants.ClientId,
            LoginRequest.ResponseType.Code
        )
        {
            Scope = Constants.MyScopes,
        };

        var uri = request.ToUri();

        return Results.Redirect(uri.ToString());
    }

    public async Task<IResult> Callback(string code)
    {
        try
        {
            var response = await new OAuthClient().RequestToken(
                new AuthorizationCodeTokenRequest(
                    Constants.ClientId,
                    Constants.ClientSecret,
                    code,
                    new Uri(Constants.RedirectUri)
                )
            );

            // Create a temporary Spotify client to get the user profile
            var tempClient = new SpotifyClient(
                SpotifyClientConfig.CreateDefault().WithToken(response.AccessToken)
            );

            var user = await tempClient.UserProfile.Current();
            if (string.IsNullOrEmpty(user.Id))
            {
                return Results.BadRequest("Login failed, retry");
            }

            // Calculate the token expiration time
            var expiresAt = DateTime.UtcNow.AddSeconds(response.ExpiresIn);

            // Generate JWT token
            var jwtToken = tokenService.GenerateJwtToken(user.Id, response.AccessToken, expiresAt);

            return Results.Ok(
                new
                {
                    token = jwtToken,
                    userId = user.Id,
                    expiresAt
                }
            );
        }
        catch (Exception ex)
        {
            return Results.BadRequest($"Authentication failed: {ex.Message}");
        }
    }
}
