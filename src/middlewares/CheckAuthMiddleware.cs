using System.Net;
using Microsoft.AspNetCore.Authentication.BearerToken;
using Newtonsoft.Json;
using SpotifyAPI.Web;

namespace tracksByPopularity.src.middlewares;

public class CheckAuthMiddleware(RequestDelegate next)
{
    /**
     * Check if client has access token. If not, get it from microservice.
     */
    public async Task InvokeAsync(HttpContext context)
    {
        if (Client.Spotify == null || string.IsNullOrEmpty(Client.Spotify.UserProfile.ToString()))
        {
            var httpClient = new HttpClient();

            var response = httpClient
                .GetAsync($"{Constants.MicroserviceAuthSpotifyBaseUrl}/token")
                .GetAwaiter()
                .GetResult();

            var stringResponse = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                context.Response.StatusCode = 401;
                return;
            }

            var tokenResponseObject = JsonConvert.DeserializeObject<AccessTokenResponse>(
                stringResponse
            );

            var accessToken = tokenResponseObject!.AccessToken;

            Client.Spotify = new SpotifyClient(accessToken);
        }

        await next(context);
    }
}
