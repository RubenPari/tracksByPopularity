using SpotifyAPI.Web;
using tracksByPopularity.src.helpers;

namespace tracksByPopularity.src.controllers;

public static class AuthController
{
    public static IResult Login()
    {
        var request = new LoginRequest(
            new Uri(Constants.RedirectUri),
            Constants.ClientId,
            LoginRequest.ResponseType.Code
        )
        {
            Scope = Constants.MyScopes
        };

        var uri = request.ToUri();

        return Results.Redirect(uri.ToString());
    }

    public static async Task<IResult> Callback(string code)
    {
        var response = await new OAuthClient().RequestToken(
            new AuthorizationCodeTokenRequest(
                Constants.ClientId,
                Constants.ClientSecret,
                code,
                new Uri(Constants.RedirectUri)
            )
        );

        Client.Spotify = new SpotifyClient(Constants.Config.WithToken(response.AccessToken));

        var user = await Client.Spotify.UserProfile.Current();

        return user.Id == string.Empty
            ? Results.BadRequest("Login failed, retry")
            : Results.Ok("Successfully authenticated!");
    }

    public static IResult Logout()
    {
        Client.Spotify = null;

        return Results.Ok("Successfully logged out!");
    }
}
