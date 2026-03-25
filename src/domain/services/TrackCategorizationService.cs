using tracksByPopularity.Domain.Entities;
using tracksByPopularity.Domain.ValueObjects;

namespace tracksByPopularity.Domain.Services;

/// <summary>
/// Domain service implementation for categorizing tracks based on popularity.
/// Contains pure business logic without infrastructure dependencies.
/// </summary>
public class TrackCategorizationService : ITrackCategorizationService
{
    /// <summary>
    /// Categorizes tracks based on their popularity into the specified range.
    /// </summary>
    /// <param name="tracks">The collection of tracks to categorize.</param>
    /// <param name="range">The popularity range to filter by.</param>
    /// <returns>A filtered list of tracks that fall within the specified popularity range.</returns>
    public IEnumerable<Track> CategorizeByPopularity(IEnumerable<Track> tracks, PopularityRange range)
    {
        return tracks.Where(track => range.Contains(track.Popularity));
    }

    /// <summary>
    /// Categorizes tracks by a specific artist into three popularity ranges.
    /// </summary>
    /// <param name="tracks">The collection of tracks to categorize.</param>
    /// <param name="artistId">The unique identifier of the artist to filter by.</param>
    /// <returns>
    /// A dictionary with keys "less", "medium", and "more", each containing
    /// the tracks in that popularity range for the specified artist.
    /// </returns>
    public Dictionary<string, List<Track>> CategorizeArtistTracks(IEnumerable<Track> tracks, string artistId)
    {
        // Filter tracks by artist
        var artistTracks = tracks
            .Where(track => track.Artists.FirstOrDefault()?.Id == artistId)
            .ToList();

        return new Dictionary<string, List<Track>>
        {
            ["less"] = artistTracks.Where(t => PopularityRange.ArtistLess.Contains(t.Popularity)).ToList(),
            ["medium"] = artistTracks.Where(t => PopularityRange.ArtistMedium.Contains(t.Popularity)).ToList(),
            ["more"] = artistTracks.Where(t => PopularityRange.ArtistMore.Contains(t.Popularity)).ToList(),
        };
    }
}

