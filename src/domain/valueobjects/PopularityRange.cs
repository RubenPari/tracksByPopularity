using tracksByPopularity.domain.exceptions;

namespace tracksByPopularity.domain.valueobjects;

/// <summary>
/// Value object representing a popularity range for categorizing tracks.
/// </summary>
public class PopularityRange
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PopularityRange"/> class.
    /// </summary>
    /// <param name="min">The minimum popularity value (inclusive).</param>
    /// <param name="max">The maximum popularity value (inclusive).</param>
    /// <exception cref="InvalidPopularityRangeException">Thrown when min is greater than max or values are out of valid range (0-100).</exception>
    public PopularityRange(int min, int max)
    {
        if (min < 0 || max > 100)
        {
            throw new InvalidPopularityRangeException($"Popularity values must be between 0 and 100. Got min={min}, max={max}");
        }

        if (min > max)
        {
            throw new InvalidPopularityRangeException(min, max);
        }

        Min = min;
        Max = max;
    }

    /// <summary>
    /// Gets the minimum popularity value (inclusive).
    /// </summary>
    public int Min { get; }

    /// <summary>
    /// Gets the maximum popularity value (inclusive).
    /// </summary>
    public int Max { get; }

    /// <summary>
    /// Determines if a popularity value falls within this range.
    /// </summary>
    /// <param name="popularity">The popularity value to check.</param>
    /// <returns><c>true</c> if the value is within the range; otherwise, <c>false</c>.</returns>
    public bool Contains(int popularity)
    {
        return popularity >= Min && popularity <= Max;
    }

    /// <summary>
    /// Creates a range for "less" popularity tracks (0-20).
    /// </summary>
    public static PopularityRange Less => new(0, 20);

    /// <summary>
    /// Creates a range for "less-medium" popularity tracks (21-40).
    /// </summary>
    public static PopularityRange LessMedium => new(21, 40);

    /// <summary>
    /// Creates a range for "medium" popularity tracks (41-60).
    /// </summary>
    public static PopularityRange Medium => new(41, 60);

    /// <summary>
    /// Creates a range for "more-medium" popularity tracks (41-80).
    /// </summary>
    public static PopularityRange MoreMedium => new(41, 80);

    /// <summary>
    /// Creates a range for "more" popularity tracks (81-100).
    /// </summary>
    public static PopularityRange More => new(81, 100);
}

