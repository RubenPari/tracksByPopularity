using Riok.Mapperly.Abstractions;
using SpotifyAPI.Web;

namespace tracksByPopularity.Application.Mapping;

[Mapper]
public partial class PlaylistMapper
{
    [MapProperty("Tracks.Total", nameof(PlaylistInfo.TotalTracks))]
    [MapProperty("Followers.Total", nameof(PlaylistInfo.Followers))]
    public partial PlaylistInfo MapToPlaylistInfo(FullPlaylist playlist);

    private static PlaylistOwnerInfo? MapOwner(PublicUser? owner) =>
        owner is null ? null : new PlaylistOwnerInfo
        {
            Id = owner.Id,
            DisplayName = owner.DisplayName,
            Uri = owner.Uri,
            Href = owner.Href,
        };

    private static PlaylistImageInfo MapImage(Image image) => new()
    {
        Url = image.Url,
        Height = image.Height,
        Width = image.Width,
    };
}