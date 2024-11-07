using tracksByPopularity.services;

namespace tracksByPopularity.controllers;

public static class AuthController
{
    public static IResult Login(SpotifyAuthService authService)
    {
        var uri = authService.GetLoginUri();
        return Results.Redirect(uri.ToString());
    }

    public static async Task<IResult> Callback(string code, SpotifyAuthService authService)
    {
        var success = await authService.HandleCallbackAsync(code);

        var user = success ? await authService.SpotifyClient!.UserProfile.Current() : null;

        return user != null && !string.IsNullOrEmpty(user.Id)
            ? Results.Ok("Successfully authenticated!")
            : Results.BadRequest("Login failed, retry");
    }

    public static IResult Logout(SpotifyAuthService authService)
    {
        authService.Logout();
        return Results.Ok("Successfully logged out!");
    }
}