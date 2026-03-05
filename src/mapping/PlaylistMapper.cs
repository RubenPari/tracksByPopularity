using Riok.Mapperly.Abstractions;
using SpotifyAPI.Web;
using tracksByPopularity.models.responses;

namespace tracksByPopularity.mapping;

[Mapper]
public partial class PlaylistMapper
{
    [MapProperty("Tracks.Total", "TotalTracks")]
    public partial PlaylistInfo MapToPlaylistInfo(FullPlaylist playlist);
}
