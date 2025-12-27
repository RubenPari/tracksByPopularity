using tracksByPopularity.models;
using tracksByPopularity.utils;

namespace tracksByPopularity.services;

/// <summary>
/// Service implementation for routing and playlist ID resolution.
/// Handles mapping of HTTP paths and query parameters to playlist IDs.
/// </summary>
public class PlaylistRoutingService : IPlaylistRoutingService
{
    private readonly IConfigurationService? _configurationService;

    /// <summary>
    /// Initializes a new instance of the <see cref="PlaylistRoutingService"/> class.
    /// </summary>
    /// <param name="configurationService">Optional configuration service for playlist IDs.</param>
    public PlaylistRoutingService(IConfigurationService? configurationService = null)
    {
        _configurationService = configurationService;
    }

    /// <summary>
    /// Determines if the given path should be handled by the playlist clearing middleware.
    /// </summary>
    /// <param name="path">The HTTP request path.</param>
    /// <returns><c>true</c> if the path should be handled; otherwise, <c>false</c>.</returns>
    /// <remarks>
    /// Paths containing "/auth" are excluded from handling.
    /// </remarks>
    public bool ShouldHandlePath(string path)
    {
        return !path.Contains("/auth", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Gets the playlist ID from the HTTP path for standard track endpoints.
    /// </summary>
    /// <param name="path">The HTTP request path.</param>
    /// <returns>
    /// The playlist ID if found; otherwise, <c>null</c>.
    /// </returns>
    /// <remarks>
    /// This method handles paths like "/track/less", "/track/less-medium", etc.
    /// </remarks>
    public string? GetPlaylistIdFromPath(string path)
    {
        // Remove the /track prefix from path if present
        if (path.StartsWith("/track", StringComparison.OrdinalIgnoreCase))
        {
            path = path.Remove(0, 6);
        }

        // Use configuration service if available, otherwise fall back to Constants
        return path switch
        {
            "/less" => _configurationService?.PlaylistSettings.PlaylistIdLess ?? Constants.PlaylistIdLess,
            "/less-medium" => _configurationService?.PlaylistSettings.PlaylistIdLessMedium ?? Constants.PlaylistIdLessMedium,
            "/more-medium" => _configurationService?.PlaylistSettings.PlaylistIdMoreMedium ?? Constants.PlaylistIdMoreMedium,
            "/more" => _configurationService?.PlaylistSettings.PlaylistIdMore ?? Constants.PlaylistIdMore,
            _ => null,
        };
    }

    /// <summary>
    /// Gets the playlist ID for a time range query parameter.
    /// </summary>
    /// <param name="timeRange">The time range enum value.</param>
    /// <returns>The playlist ID corresponding to the time range.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when the time range is not valid.
    /// </exception>
    public string GetPlaylistIdForTimeRange(TimeRangeEnum timeRange)
    {
        // Use configuration service if available, otherwise fall back to Constants
        return timeRange switch
        {
            TimeRangeEnum.ShortTerm => _configurationService?.PlaylistSettings.PlaylistIdTopShort ?? Constants.PlaylistIdTopShort,
            TimeRangeEnum.MediumTerm => _configurationService?.PlaylistSettings.PlaylistIdTopMedium ?? Constants.PlaylistIdTopMedium,
            TimeRangeEnum.LongTerm => _configurationService?.PlaylistSettings.PlaylistIdTopLong ?? Constants.PlaylistIdTopLong,
            _ => throw new ArgumentOutOfRangeException(nameof(timeRange), timeRange, "Invalid time range"),
        };
    }
}

