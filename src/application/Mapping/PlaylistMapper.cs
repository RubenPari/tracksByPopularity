using Riok.Mapperly.Abstractions;
using SpotifyAPI.Web;
using tracksByPopularity.Application.DTOs;

namespace tracksByPopularity.Application.Mapping;

[Mapper]
public partial class PlaylistMapper
{
    [MapProperty("Tracks.Total", "TotalTracks")]
    public partial PlaylistInfo MapToPlaylistInfo(FullPlaylist playlist);
}
