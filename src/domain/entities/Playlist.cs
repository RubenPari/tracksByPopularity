namespace tracksByPopularity.domain.entities;

/// <summary>
/// Domain entity representing a playlist.
/// </summary>
public class Playlist
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
    /// Gets or sets the list of track IDs in the playlist.
    /// </summary>
    public List<string> TrackIds { get; set; } = new();
}

