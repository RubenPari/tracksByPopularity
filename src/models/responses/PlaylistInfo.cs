namespace tracksByPopularity.models.responses;

/// <summary>
/// Represents basic information about a Spotify playlist.
/// </summary>
public class PlaylistInfo
{
    /// <summary>
    /// Gets or sets the unique identifier of the playlist.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the name of the playlist.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the description of the playlist.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the total number of tracks in the playlist.
    /// </summary>
    public int TotalTracks { get; set; }

    /// <summary>
    /// Gets or sets the URI of the playlist.
    /// </summary>
    public string? Uri { get; set; }
}

