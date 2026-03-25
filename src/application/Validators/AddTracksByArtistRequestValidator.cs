using FluentValidation;

namespace tracksByPopularity.Application.Validators;

/// <summary>
/// Validator for <see cref="AddTracksByArtistRequest"/>.
/// Validates that the artist ID is in the correct Spotify format.
/// </summary>
public abstract class AddTracksByArtistRequestValidator : AbstractValidator<AddTracksByArtistRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AddTracksByArtistRequestValidator"/> class.
    /// </summary>
    protected AddTracksByArtistRequestValidator()
    {
        RuleFor(x => x.ArtistId)
            .NotEmpty()
            .WithMessage("Artist ID is required")
            .Length(22)
            .WithMessage("Artist ID must be exactly 22 characters")
            .Matches("^[a-zA-Z0-9]{22}$")
            .WithMessage("Artist ID must contain only alphanumeric characters");
    }
}

