using tracksByPopularity.domain.entities;
using tracksByPopularity.domain.valueobjects;

namespace tracksByPopularity.domain.services;

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
            .Where(track => track.Artists.Any(artist => artist.Id == artistId))
            .ToList();

        // Define artist-specific popularity ranges
        var lessRange = new PopularityRange(0, 33);
        var mediumRange = new PopularityRange(34, 66);
        var moreRange = new PopularityRange(67, 100);

        return new Dictionary<string, List<Track>>
        {
            ["less"] = artistTracks.Where(t => lessRange.Contains(t.Popularity)).ToList(),
            ["medium"] = artistTracks.Where(t => mediumRange.Contains(t.Popularity)).ToList(),
            ["more"] = artistTracks.Where(t => moreRange.Contains(t.Popularity)).ToList(),
        };
    }
}

