namespace tracksByPopularity.Application.DTOs;

public class PlaylistSnapshot
{
    public required string Id { get; set; }
    public required string PlaylistId { get; set; }
    public required string PlaylistName { get; set; }
    public required string OperationType { get; set; }
    public required DateTime CreatedAt { get; set; }
    public required int TrackCount { get; set; }
    public required List<string> TrackUris { get; set; }
}
