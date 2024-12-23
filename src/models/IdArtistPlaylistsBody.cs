namespace tracksByPopularity.models;

public class IdArtistPlaylistsBody(string less, string lessMedium, string moreMedium, string more)
{
    // <summary>
    // 0 <= 25
    // </summary>
    public required string Less { get; set; } =
        less ?? throw new ArgumentNullException(nameof(less));

    // <summary>
    // 25 < x <= 50
    // </summary>
    public required string LessMedium { get; set; } =
        lessMedium ?? throw new ArgumentNullException(nameof(lessMedium));

    // <summary>
    // 50 < x <= 75
    // </summary>
    public required string MoreMedium { get; set; } =
        moreMedium ?? throw new ArgumentNullException(nameof(moreMedium));

    // <summary>
    // 75 < x <= 100
    // </summary>
    public required string More { get; set; } =
        more ?? throw new ArgumentNullException(nameof(more));
}
