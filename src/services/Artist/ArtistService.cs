using SpotifyAPI.Web;
using tracksByPopularity.models;

namespace tracksByPopularity.services;

public static class ArtistService
{
    public static List<ArtistsSummary> GetArtistsSummary(IList<SavedTrack> tracks)
    {
        // return a list of object with artist name, artist id and number of tracks of the artist

        var artists = tracks
            .Select(track => track.Track.Artists)
            .SelectMany(artists => artists)
            .Distinct()
            .ToList();

        var artistsSummary = artists
            .Select(artist =>
            {
                return new ArtistsSummary
                {
                    Id = artist.Id,
                    Name = artist.Name,
                    Count = tracks
                        .Count(track => track.Track.Artists.Any(a => a.Id == artist.Id))
                };
            })
            .ToList();

        return artistsSummary;
    }
}