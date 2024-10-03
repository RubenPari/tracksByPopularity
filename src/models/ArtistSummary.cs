namespace tracksByPopularity.models;

public abstract class ArtistSummary
{
    public required string Id { get; set; }
    public required string Name { get; set; }
    public required int Count { get; set; }
}
