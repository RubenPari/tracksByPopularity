namespace tracksByPopularity.Domain.Entities;

public class SnapshotTrack
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid SnapshotId { get; set; }
    public string TrackUri { get; set; } = string.Empty;
    public int OrderIndex { get; set; }

    public PlaylistSnapshot? Snapshot { get; set; }
}
