using tracksByPopularity.models;

namespace tracksByPopularity.services;

public interface IArtistService
{
    Task<IEnumerable<ArtistSummary>?> GetArtistsSummaryAsync();
}

