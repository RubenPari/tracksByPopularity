import { AppError } from "../lib/response";
import {
  findOrCreatePlaylist,
  getFollowedArtists,
  getSavedTracks,
  replacePlaylistTracks,
  type Track,
} from "./library";
import { createSnapshot } from "./snapshot";

type Range = readonly [min: number, max: number];

export const POPULARITY_RANGES = {
  less: [0, 20],
  "less-medium": [21, 40],
  medium: [41, 60],
  "more-medium": [61, 80],
  more: [81, 100],
} as const satisfies Record<string, Range>;
export type PopularityRange = keyof typeof POPULARITY_RANGES;

export const ARTIST_RANGES = {
  less: [0, 33],
  medium: [34, 66],
  more: [67, 100],
} as const satisfies Record<string, Range>;

export const inRange = (track: Track, [min, max]: Range) => track.popularity >= min && track.popularity <= max;

export const popularityPlaylistName = (range: PopularityRange) => {
  const [min, max] = POPULARITY_RANGES[range];
  return `Popularity: ${min}-${max}`;
};

/** Splits an artist's tracks into the three artist bands (pure, for testing). */
export function splitByArtistRanges(tracks: Track[]) {
  return Object.entries(ARTIST_RANGES).map(([band, range]) => ({
    band,
    uris: tracks.filter((track) => inRange(track, range)).map((track) => track.uri),
  }));
}

export async function sortByPopularity(spotifyUserId: string, range: PopularityRange) {
  const playlist = await findOrCreatePlaylist(spotifyUserId, popularityPlaylistName(range));
  await createSnapshot(spotifyUserId, playlist, "popularity_sort");
  const uris = (await getSavedTracks(spotifyUserId))
    .filter((track) => inRange(track, POPULARITY_RANGES[range]))
    .map((track) => track.uri);
  await replacePlaylistTracks(spotifyUserId, playlist.id, uris);
  return { playlistId: playlist.id, playlistName: playlist.name, trackCount: uris.length };
}

/** Followed artists present in the saved library, sorted by number of saved tracks (desc). */
export async function listLibraryArtists(spotifyUserId: string) {
  const [tracks, followed] = await Promise.all([getSavedTracks(spotifyUserId), getFollowedArtists(spotifyUserId)]);
  const counts = new Map<string, number>();
  for (const track of tracks) for (const artist of track.artists) counts.set(artist.id, (counts.get(artist.id) ?? 0) + 1);
  return followed
    .filter((artist) => counts.has(artist.id))
    .map((artist) => ({ ...artist, trackCount: counts.get(artist.id)! }))
    .sort((a, b) => b.trackCount - a.trackCount);
}

export async function splitArtist(spotifyUserId: string, artistId: string) {
  const tracks = (await getSavedTracks(spotifyUserId)).filter((track) => track.artists.some((a) => a.id === artistId));
  const artistName = tracks[0]?.artists.find((a) => a.id === artistId)?.name;
  if (!artistName) throw new AppError(404, "ARTIST_NOT_FOUND", "Nessun brano salvato per questo artista");

  const bands = splitByArtistRanges(tracks);
  const playlists = [];
  for (const { band } of bands) playlists.push(await findOrCreatePlaylist(spotifyUserId, `${artistName} ${band}`));
  // Snapshot all three before touching any of them.
  for (const playlist of playlists) await createSnapshot(spotifyUserId, playlist, "artist_split");
  const result = [];
  for (const [i, { band, uris }] of bands.entries()) {
    await replacePlaylistTracks(spotifyUserId, playlists[i]!.id, uris);
    result.push({ band, playlistId: playlists[i]!.id, playlistName: playlists[i]!.name, trackCount: uris.length });
  }
  return { artistId, artistName, playlists: result };
}
