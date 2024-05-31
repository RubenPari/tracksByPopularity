namespace tracksByPopularity.models
{
    public abstract class IdArtistPlaylists
    {
        public required string IdArtistPlaylistLess { get; set; }
        public required string IdArtistPlaylistMedium { get; set; }
        public required string IdArtistPlaylistMore { get; set; }
    }
}
