using tracksByPopularity.Domain.Enums;

namespace tracksByPopularity.Domain.Services;

/// <summary>
/// Domain service interface for analyzing track moods based on audio features.
/// </summary>
public interface ITrackMoodAnalyzerService
{
    /// <summary>
    /// Determines the mood category based on valence and energy.
    /// </summary>
    /// <param name="valence">A measure from 0.0 to 1.0 describing the musical positiveness conveyed by a track.</param>
    /// <param name="energy">A measure from 0.0 to 1.0 representing a perceptual measure of intensity and activity.</param>
    /// <returns>The calculated MoodCategory, or null if it doesn't strongly fit any category.</returns>
    MoodCategory? AnalyzeMood(float valence, float energy);
}
