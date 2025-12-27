namespace tracksByPopularity.application.exceptions;

/// <summary>
/// Exception thrown when a playlist operation fails.
/// </summary>
public class PlaylistOperationException : Exception
{
    /// <summary>
    /// Gets the playlist ID associated with the failed operation.
    /// </summary>
    public string? PlaylistId { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="PlaylistOperationException"/> class.
    /// </summary>
    public PlaylistOperationException()
        : base("Playlist operation failed")
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PlaylistOperationException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public PlaylistOperationException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PlaylistOperationException"/> class with a playlist ID and error message.
    /// </summary>
    /// <param name="playlistId">The playlist ID associated with the failed operation.</param>
    /// <param name="message">The message that describes the error.</param>
    public PlaylistOperationException(string playlistId, string message)
        : base(message)
    {
        PlaylistId = playlistId;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PlaylistOperationException"/> class with a specified error message
    /// and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public PlaylistOperationException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}

