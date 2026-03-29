namespace tracksByPopularity.Domain.Entities;

public class PlaylistSnapshot
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid? UserId { get; set; }
    public string SpotifyUserId { get; set; } = string.Empty;
    public string PlaylistId { get; set; } = string.Empty;
    public string PlaylistName { get; set; } = string.Empty;
    public string OperationType { get; set; } = string.Empty;
    public int TrackCount { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User? User { get; set; }
    public ICollection<SnapshotTrack> Tracks { get; set; } = new List<SnapshotTrack>();
}
