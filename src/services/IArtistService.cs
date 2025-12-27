using tracksByPopularity.models;

namespace tracksByPopularity.services;

/// <summary>
/// Service interface for artist-related operations.
/// Provides methods to retrieve artist summary information.
/// </summary>
public interface IArtistService
{
    /// <summary>
    /// Retrieves a summary of all artists in the user's library, including
    /// the count of tracks for each artist.
    /// </summary>
    /// <returns>
    /// An enumerable of <see cref="ArtistSummary"/> objects containing artist ID, name, and track count,
    /// or <c>null</c> if the request fails or the external service is unavailable.
    /// </returns>
    /// <remarks>
    /// This method calls an external service endpoint to retrieve artist summary data.
    /// The summary includes information about how many tracks each artist has in the user's library.
    /// </remarks>
    Task<IEnumerable<ArtistSummary>?> GetArtistsSummaryAsync();
}

