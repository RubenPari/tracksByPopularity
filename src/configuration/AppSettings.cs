namespace tracksByPopularity.configuration;

public class AppSettings
{
    public string TitleApi { get; set; } = "TracksByPopularityAPI";
    
    public int TracksLessPopularity { get; set; } = 20;
    public int TracksLessMediumPopularity { get; set; } = 40;
    public int TracksMediumPopularity { get; set; } = 60;
    public int TracksMoreMediumPopularity { get; set; } = 80;
    public int ArtistTracksLessPopularity { get; set; } = 33;
    public int ArtistTracksMediumPopularity { get; set; } = 66;

    public string PlaylistNameWithMinorSongs { get; set; } = "MinorSongs";
    public int Offset { get; set; } = 0;
    public int LimitInsertPlaylistTracks { get; set; } = 100;
}

