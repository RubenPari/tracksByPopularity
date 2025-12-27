namespace tracksByPopularity.models.requests;

/// <summary>
/// Request model for adding tracks by artist to playlists.
/// </summary>
public class AddTracksByArtistRequest
{
    /// <summary>
    /// Gets or sets the unique identifier of the artist.
    /// Must be exactly 22 alphanumeric characters (Spotify ID format).
    /// </summary>
    public string ArtistId { get; set; } = string.Empty;
}

