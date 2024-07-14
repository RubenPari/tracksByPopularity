using SpotifyAPI.Web;

namespace tracksByPopularity.controllers;

public static class AuthController
{
    public static IResult Login()
    {
        var request = new LoginRequest(
            new Uri(Costants.RedirectUri),
            Costants.ClientId,
            LoginRequest.ResponseType.Code
        )
        {
            Scope = Costants.MyScopes
        };

        var uri = request.ToUri();

        return Results.Redirect(uri.ToString());
    }

    public static async Task<IResult> Callback(string code)
    {
        var response = await new OAuthClient().RequestToken(
            new AuthorizationCodeTokenRequest(
                Costants.ClientId,
                Costants.ClientSecret,
                code,
                new Uri(Costants.RedirectUri)
            )
        );

        Client.Spotify = new SpotifyClient(Costants.Config.WithToken(response.AccessToken));

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
