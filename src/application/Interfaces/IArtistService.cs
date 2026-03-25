namespace tracksByPopularity.Application.Interfaces;

/// <summary>
/// Service interface for artist-related operations.
/// Provides methods to retrieve artist summary information.
/// </summary>
public interface IArtistService
{
    // TODO: non viene usato da nessuno
    /// <summary>
    /// Retrieves a summary of all artists in the user's library from an external service.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task result contains a collection of artist summaries.</returns>
    public Task<IEnumerable<ArtistSummary>?> GetArtistsSummaryAsync();
}

