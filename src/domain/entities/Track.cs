namespace tracksByPopularity.domain.entities;

/// <summary>
/// Domain entity representing a track.
/// This is a domain model that is independent of external frameworks and libraries.
/// </summary>
public class Track
{
    /// <summary>
    /// Gets or sets the unique identifier of the track.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the name of the track.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the popularity score of the track (0-100).
    /// </summary>
    public int Popularity { get; set; }

    /// <summary>
    /// Gets or sets the URI of the track.
    /// </summary>
    public string Uri { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the list of artists associated with this track.
    /// </summary>
    public List<Artist> Artists { get; set; } = new();
}

/// <summary>
/// Domain entity representing an artist.
/// </summary>
public class Artist
{
    /// <summary>
    /// Gets or sets the unique identifier of the artist.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the name of the artist.
    /// </summary>
    public string Name { get; set; } = string.Empty;
}

