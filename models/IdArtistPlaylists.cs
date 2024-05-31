namespace tracksByPopularity.models
{
    public class IdArtistPlaylists
    {
        public required string IdArtistPlaylistLess { get; set; }
        public required string IdArtistPlaylistMedium { get; set; }
        public required string IdArtistPlaylistMore { get; set; }

        public IdArtistPlaylists() { }
    }
}
