/** Minimal Spotify Web API shapes used at the library boundary. */

export type SpotifyImage = { url: string };
export type SpotifyArtistRef = { id: string; name: string };

export type SpotifyPaging<T> = {
  items: T[];
  next: string | null;
};

export type SpotifyTrack = {
  id: string;
  uri: string;
  name: string;
  popularity?: number;
  is_local?: boolean;
  artists: SpotifyArtistRef[];
};

export type SpotifySavedTrackItem = { track: SpotifyTrack | null };

export type SpotifyArtist = {
  id: string;
  name: string;
  popularity?: number;
  images?: SpotifyImage[];
};

export type SpotifyFollowedArtistsPage = {
  artists: SpotifyPaging<SpotifyArtist>;
};

export type SpotifyPlaylist = {
  id: string;
  name: string;
  owner?: { id: string };
  tracks?: { total?: number };
  images?: SpotifyImage[];
};

export type SpotifyPlaylistTrackItem = {
  track: { uri?: string; is_local?: boolean } | null;
};

export type SpotifyProfile = {
  id: string;
  display_name?: string | null;
  images?: SpotifyImage[];
};
