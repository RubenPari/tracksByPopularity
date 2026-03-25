namespace tracksByPopularity.Application.DTOs;

/// <summary>
/// Represents basic information about a Spotify playlist.
/// </summary>
public class PlaylistInfo
{
    /// <summary>
    /// Gets or sets the unique identifier of the playlist.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the name of the playlist.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the description of the playlist.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the total number of tracks in the playlist.
    /// </summary>
    public int TotalTracks { get; set; }

    /// <summary>
    /// Gets or sets the URI of the playlist.
    /// </summary>
    public string? Uri { get; set; }

    /// <summary>
    /// Gets or sets whether the playlist is collaborative.
    /// </summary>
    public bool? Collaborative { get; set; }

    /// <summary>
    /// Gets or sets the external URLs for the playlist (e.g. Spotify web link).
    /// </summary>
    public Dictionary<string, string>? ExternalUrls { get; set; }

    /// <summary>
    /// Gets or sets the follower count of the playlist.
    /// </summary>
    public int? Followers { get; set; }

    /// <summary>
    /// Gets or sets the Spotify API href of the playlist.
    /// </summary>
    public string? Href { get; set; }

    /// <summary>
    /// Gets or sets the cover images of the playlist.
    /// </summary>
    public List<PlaylistImageInfo>? Images { get; set; }

    /// <summary>
    /// Gets or sets the owner of the playlist.
    /// </summary>
    public PlaylistOwnerInfo? Owner { get; set; }

    /// <summary>
    /// Gets or sets whether the playlist is public.
    /// </summary>
    public bool? Public { get; set; }

    /// <summary>
    /// Gets or sets the snapshot ID of the playlist (used for change tracking).
    /// </summary>
    public string? SnapshotId { get; set; }

    /// <summary>
    /// Gets or sets the object type (always "playlist").
    /// </summary>
    public string? Type { get; set; }
}

/// <summary>
/// Represents a playlist cover image.
/// </summary>
public class PlaylistImageInfo
{
    public string? Url { get; set; }
    public int? Height { get; set; }
    public int? Width { get; set; }
}

/// <summary>
/// Represents the owner of a playlist.
/// </summary>
public class PlaylistOwnerInfo
{
    public string? Id { get; set; }
    public string? DisplayName { get; set; }
    public string? Uri { get; set; }
    public string? Href { get; set; }
}