using Riok.Mapperly.Abstractions;
using SpotifyAPI.Web;
using tracksByPopularity.Application.DTOs;

namespace tracksByPopularity.Application.Mapping;

[Mapper]
public partial class PlaylistMapper
{
    [MapProperty(nameof(FullPlaylist.Tracks) + "." + nameof(Paging<FullTrack>.Total), nameof(PlaylistInfo.TotalTracks))]
    public partial PlaylistInfo MapToPlaylistInfo(FullPlaylist playlist);
}
