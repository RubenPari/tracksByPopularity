using SpotifyAPI.Web;
using tracksByPopularity.domain.entities;

namespace tracksByPopularity.infrastructure.mappers;

/// <summary>
/// Mapper for converting between Spotify API models and domain entities.
/// This isolates infrastructure concerns (Spotify SDK) from domain logic.
/// </summary>
public static class SpotifyTrackMapper
{
    /// <summary>
    /// Converts a Spotify SavedTrack to a domain Track entity.
    /// </summary>
    /// <param name="savedTrack">The Spotify SavedTrack to convert.</param>
    /// <returns>A domain Track entity.</returns>
    public static Track ToDomain(SavedTrack savedTrack)
    {
        return new Track
        {
            Id = savedTrack.Track.Id,
            Name = savedTrack.Track.Name,
            Popularity = savedTrack.Track.Popularity ?? 0,
            Uri = savedTrack.Track.Uri,
            Artists = savedTrack.Track.Artists.Select(artist => new Artist
            {
                Id = artist.Id,
                Name = artist.Name,
            }).ToList(),
        };
    }

    /// <summary>
    /// Converts a collection of Spotify SavedTracks to domain Track entities.
    /// </summary>
    /// <param name="savedTracks">The collection of Spotify SavedTracks to convert.</param>
    /// <returns>A collection of domain Track entities.</returns>
    public static IEnumerable<Track> ToDomain(IEnumerable<SavedTrack> savedTracks)
    {
        return savedTracks.Select(ToDomain);
    }

    /// <summary>
    /// Extracts URIs from a collection of domain Track entities.
    /// </summary>
    /// <param name="tracks">The collection of domain Track entities.</param>
    /// <returns>A list of track URIs.</returns>
    public static List<string> ExtractUris(IEnumerable<Track> tracks)
    {
        return tracks.Select(track => track.Uri).ToList();
    }
}

