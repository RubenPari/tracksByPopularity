namespace tracksByPopularity.Infrastructure.Configuration;

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
    /// Gets or sets the initial offset for pagination when adding tracks to playlists.
    /// </summary>
    public int Offset { get; init; }

    /// <summary>
    /// Gets or sets the maximum number of tracks to add per batch when inserting into playlists.
    /// This should not exceed Spotify's API limit of 100 tracks per request.
    /// </summary>
    public int LimitInsertPlaylistTracks { get; init; } = 100;

    /// <summary>
    /// Gets or sets the frontend origin URL used for OAuth redirects.
    /// </summary>
    public string FrontendOrigin { get; set; } = "http://127.0.0.1:5173";

    /// <summary>
    /// Gets or sets the base URL for the ClearSongs external service.
    /// </summary>
    public string ClearSongsBaseUrl { get; set; } = "http://localhost:3000";

    /// <summary>
    /// Gets or sets the base URL for the track summary external service.
    /// </summary>
    public string TrackSummaryBaseUrl { get; set; } = "http://localhost:3030";
}

