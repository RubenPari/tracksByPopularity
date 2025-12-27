using tracksByPopularity.domain.entities;
using tracksByPopularity.domain.valueobjects;

namespace tracksByPopularity.domain.services;

/// <summary>
/// Domain service for categorizing tracks based on popularity.
/// This service contains business logic that is independent of infrastructure concerns.
/// </summary>
public interface ITrackCategorizationService
{
    /// <summary>
    /// Categorizes tracks based on their popularity into the specified range.
    /// </summary>
    /// <param name="tracks">The collection of tracks to categorize.</param>
    /// <param name="range">The popularity range to filter by.</param>
    /// <returns>A filtered list of tracks that fall within the specified popularity range.</returns>
    IEnumerable<Track> CategorizeByPopularity(IEnumerable<Track> tracks, PopularityRange range);

    /// <summary>
    /// Categorizes tracks by a specific artist into three popularity ranges:
    /// less (≤33), medium (34-66), and more (>66).
    /// </summary>
    /// <param name="tracks">The collection of tracks to categorize.</param>
    /// <param name="artistId">The unique identifier of the artist to filter by.</param>
    /// <returns>
    /// A dictionary with keys "less", "medium", and "more", each containing
    /// the tracks in that popularity range for the specified artist.
    /// </returns>
    Dictionary<string, List<Track>> CategorizeArtistTracks(IEnumerable<Track> tracks, string artistId);
}

