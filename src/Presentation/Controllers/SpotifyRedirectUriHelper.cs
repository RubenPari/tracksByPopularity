namespace tracksByPopularity.Presentation.Controllers;

internal static class SpotifyRedirectUriHelper
{
    private const string AuthCallbackPath = "/auth/callback";
    private const string LinkCallbackPath = "/api/spotify/callback";

    public static Uri GetAuthCallbackUri(string configuredRedirectUri)
        => BuildCallbackUri(configuredRedirectUri, AuthCallbackPath);

    public static Uri GetLinkCallbackUri(string configuredRedirectUri)
        => BuildCallbackUri(configuredRedirectUri, LinkCallbackPath);

    private static Uri BuildCallbackUri(string configuredRedirectUri, string callbackPath)
    {
        if (string.IsNullOrWhiteSpace(configuredRedirectUri))
        {
            throw new InvalidOperationException("Spotify RedirectUri configuration is missing.");
        }

        var normalizedBaseUri = configuredRedirectUri.TrimEnd('/');

        if (normalizedBaseUri.EndsWith(AuthCallbackPath, StringComparison.OrdinalIgnoreCase))
        {
            normalizedBaseUri = normalizedBaseUri[..^AuthCallbackPath.Length];
        }
        else if (normalizedBaseUri.EndsWith(LinkCallbackPath, StringComparison.OrdinalIgnoreCase))
        {
            normalizedBaseUri = normalizedBaseUri[..^LinkCallbackPath.Length];
        }

        return new Uri($"{normalizedBaseUri}{callbackPath}", UriKind.Absolute);
    }
}
