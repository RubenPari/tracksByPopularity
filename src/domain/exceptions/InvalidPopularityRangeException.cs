namespace tracksByPopularity.domain.exceptions;

/// <summary>
/// Exception thrown when an invalid popularity range is specified.
/// </summary>
public class InvalidPopularityRangeException : DomainException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="InvalidPopularityRangeException"/> class.
    /// </summary>
    public InvalidPopularityRangeException()
        : base("Invalid popularity range specified")
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="InvalidPopularityRangeException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public InvalidPopularityRangeException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="InvalidPopularityRangeException"/> class with min and max values.
    /// </summary>
    /// <param name="min">The minimum value that was specified.</param>
    /// <param name="max">The maximum value that was specified.</param>
    public InvalidPopularityRangeException(int min, int max)
        : base($"Invalid popularity range: min ({min}) cannot be greater than max ({max})")
    {
    }
}

