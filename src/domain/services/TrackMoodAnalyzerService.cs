using tracksByPopularity.Domain.Enums;

namespace tracksByPopularity.Domain.Services;

/// <summary>
/// Domain service implementation for analyzing track moods.
/// </summary>
public class TrackMoodAnalyzerService : ITrackMoodAnalyzerService
{
    public MoodCategory? AnalyzeMood(float valence, float energy)
    {
        // Happy & Upbeat: High positivity, high energy
        if (valence >= 0.6f && energy >= 0.6f)
        {
            return MoodCategory.HappyAndUpbeat;
        }

        // High Energy Workout: Lower positivity, but very high energy (Intense/Aggressive)
        if (valence < 0.6f && energy >= 0.7f)
        {
            return MoodCategory.HighEnergy;
        }

        // Chill & Relaxing: Positive, but low energy (Acoustic/Chillout)
        if (valence >= 0.5f && energy <= 0.5f)
        {
            return MoodCategory.ChillAndRelaxing;
        }

        // Melancholic & Deep: Low positivity, low energy (Sad/Dark)
        if (valence <= 0.4f && energy <= 0.5f)
        {
            return MoodCategory.SadAndMelancholic;
        }

        // If it falls in the middle or doesn't strongly match, return null
        return null;
    }
}
