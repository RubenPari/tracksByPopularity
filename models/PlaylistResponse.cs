namespace tracksByPopularity.models;

public class PlaylistResponse
{
    public required List<PlaylistItem> Items { get; set; }
    public required string Next { get; set; }
}