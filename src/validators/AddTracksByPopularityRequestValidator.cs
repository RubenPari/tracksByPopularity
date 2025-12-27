using FluentValidation;
using tracksByPopularity.models.requests;

namespace tracksByPopularity.validators;

/// <summary>
/// Validator for <see cref="AddTracksByPopularityRequest"/>.
/// Ensures that the PlaylistId is a valid Spotify playlist ID.
/// </summary>
public class AddTracksByPopularityRequestValidator : AbstractValidator<AddTracksByPopularityRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AddTracksByPopularityRequestValidator"/> class.
    /// </summary>
    public AddTracksByPopularityRequestValidator()
    {
        RuleFor(x => x.PlaylistId)
            .NotEmpty()
            .WithMessage("Playlist ID is required.")
            .Length(22)
            .WithMessage("Playlist ID must be 22 characters long.")
            .Matches("^[a-zA-Z0-9]+$")
            .WithMessage("Playlist ID must be alphanumeric.");
    }
}

