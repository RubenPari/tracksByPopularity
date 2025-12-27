namespace tracksByPopularity.models.requests;

/// <summary>
/// Represents a request to add tracks to a playlist based on popularity range.
/// </summary>
public class AddTracksByPopularityRequest
{
    /// <summary>
    /// Gets or sets the Spotify ID of the playlist to add tracks to.
    /// </summary>
    public required string PlaylistId { get; set; }
}

