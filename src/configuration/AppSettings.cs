namespace tracksByPopularity.configuration;

/// <summary>
/// Application settings configuration class.
/// Contains threshold values for track popularity categorization and playlist management settings.
/// </summary>
public class AppSettings
{
    /// <summary>
    /// Gets or sets the API title/name.
    /// </summary>
    public string TitleApi { get; init; } = "TracksByPopularityAPI";
    
    /// <summary>
    /// Gets or sets the maximum popularity value for "less" category tracks (0-20).
    /// </summary>
    public int TracksLessPopularity { get; init; } = 20;
    
    /// <summary>
    /// Gets or sets the maximum popularity value for "less-medium" category tracks (21-40).
    /// </summary>
    public int TracksLessMediumPopularity { get; init; } = 40;
    
    /// <summary>
    /// Gets or sets the maximum popularity value for "medium" category tracks (41-60).
    /// </summary>
    public int TracksMediumPopularity { get; init; } = 60;
    
    /// <summary>
    /// Gets or sets the maximum popularity value for "more-medium" category tracks (41-80).
    /// </summary>
    public int TracksMoreMediumPopularity { get; init; } = 80;
    
    /// <summary>
    /// Gets or sets the maximum popularity value for artist tracks in "less" category (0-33).
    /// </summary>
    public int ArtistTracksLessPopularity { get; init; } = 33;
    
    /// <summary>
    /// Gets or sets the maximum popularity value for artist tracks in "medium" category (34-66).
    /// </summary>
    public int ArtistTracksMediumPopularity { get; init; } = 66;

    /// <summary>
    /// Gets or sets the name of the playlist for tracks from artists with ≤5 songs.
    /// </summary>
    public string PlaylistNameWithMinorSongs { get; init; } = "MinorSongs";
    
    /// <summary>
    /// Gets or sets the initial offset for pagination when adding tracks to playlists.
    /// </summary>
    public int Offset { get; init; }
    
    /// <summary>
    /// Gets or sets the maximum number of tracks to add per batch when inserting into playlists.
    /// This should not exceed Spotify's API limit of 100 tracks per request.
    /// </summary>
    public int LimitInsertPlaylistTracks { get; init; } = 100;
}

