namespace tracksByPopularity.Application.DTOs;

/// <summary>
/// Request model for adding tracks by artist to playlists.
/// - ArtistId: The ID of the artist whose tracks are to be added.
/// </summary>
public class AddTracksByArtistRequest
{
    public string ArtistId { get; set; } = string.Empty;
}

