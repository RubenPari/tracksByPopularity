using tracksByPopularity.models;

namespace tracksByPopularity.services;

/// <summary>
/// Service interface for routing and playlist ID resolution.
/// Handles mapping of HTTP paths and query parameters to playlist IDs.
/// </summary>
public interface IPlaylistRoutingService
{
    /// <summary>
    /// Determines if the given path should be handled by the playlist clearing middleware.
    /// </summary>
    /// <param name="path">The HTTP request path.</param>
    /// <returns><c>true</c> if the path should be handled; otherwise, <c>false</c>.</returns>
    bool ShouldHandlePath(string path);

    /// <summary>
    /// Gets the playlist ID from the HTTP path for standard track endpoints.
    /// </summary>
    /// <param name="path">The HTTP request path.</param>
    /// <returns>
    /// The playlist ID if found; otherwise, <c>null</c>.
    /// </returns>
    string? GetPlaylistIdFromPath(string path);

    /// <summary>
    /// Gets the playlist ID for a time range query parameter.
    /// </summary>
    /// <param name="timeRange">The time range enum value.</param>
    /// <returns>The playlist ID corresponding to the time range.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when the time range is not valid.
    /// </exception>
    string GetPlaylistIdForTimeRange(TimeRangeEnum timeRange);
}

