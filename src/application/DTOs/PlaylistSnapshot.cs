namespace tracksByPopularity.Application.DTOs;

/// <summary>
/// Represents a snapshot of a playlist at a specific point in time.
/// </summary>
public class PlaylistSnapshot
{
    /// <summary>
    /// The unique identifier for the snapshot.
    /// </summary>
    public required string Id { get; init; }
    
    /// <summary>
    /// The unique identifier for the playlist.
    /// </summary>
    public required string PlaylistId { get; init; }
    
    /// <summary>
    /// The name of the playlist.
    /// </summary>
    public required string PlaylistName { get; init; }
    
    /// <summary>
    /// The type of operation that was performed to create the snapshot.
    /// </summary>
    public required string OperationType { get; init; }
    
    /// <summary>
    /// The date and time when the snapshot was created.
    /// </summary>
    public required DateTime CreatedAt { get; init; }
    
    /// <summary>
    /// The number of tracks in the playlist.
    /// </summary>
    public required int TrackCount { get; init; }
    
    /// <summary>
    /// The list of track URIs in the playlist.
    /// </summary>
    public required List<string> TrackUris { get; init; }
}
