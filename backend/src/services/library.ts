import { cache, keys, TTL } from "../lib/cache";
import { paginate, spotifyFetch } from "../spotify/client";

/** Cached Spotify library reads/writes: profile, saved tracks, artists, playlists, and track replace. */

export type Track = { id: string; uri: string; name: string; popularity: number; artists: { id: string; name: string }[] };
export type Artist = { id: string; name: string; popularity: number; image: string | null };
export type Playlist = { id: string; name: string; trackCount: number; image: string | null };

export type Profile = { displayName: string; image: string | null };

/** Cached `/me` profile (display name + first image). */
export function getProfile(spotifyUserId: string): Promise<Profile> {
  return cache.wrap(keys.profile(spotifyUserId), TTL.profile, async () => {
    const me = await spotifyFetch<any>(spotifyUserId, "/me");
    return { displayName: me.display_name ?? me.id, image: me.images?.[0]?.url ?? null };
  });
}

/** Cached saved library tracks; skips local files that lack a Spotify uri. */
export function getSavedTracks(spotifyUserId: string): Promise<Track[]> {
  return cache.wrap(keys.tracks(spotifyUserId), TTL.tracks, () =>
    paginate<Track>(spotifyUserId, "/me/tracks?limit=50", (page) => ({
      next: page.next,
      items: page.items
        .map((item: any) => item.track)
        .filter((track: any) => track && !track.is_local)
        .map((track: any) => ({
          id: track.id,
          uri: track.uri,
          name: track.name,
          popularity: track.popularity ?? 0,
          artists: track.artists.map((a: any) => ({ id: a.id, name: a.name })),
        })),
    })),
  );
}

/** Cached followed artists with popularity and primary image. */
export function getFollowedArtists(spotifyUserId: string): Promise<Artist[]> {
  return cache.wrap(keys.artists(spotifyUserId), TTL.artists, () =>
    paginate<Artist>(spotifyUserId, "/me/following?type=artist&limit=50", (page) => ({
      next: page.artists.next,
      items: page.artists.items.map((a: any) => ({
        id: a.id,
        name: a.name,
        popularity: a.popularity ?? 0,
        image: a.images?.[0]?.url ?? null,
      })),
    })),
  );
}

/** Fresh playlist list owned by the user (not cached). */
export async function fetchPlaylists(spotifyUserId: string): Promise<Playlist[]> {
  const playlists = await paginate<any>(spotifyUserId, "/me/playlists?limit=50");
  // Spotify returns collaborative/followed playlists too; keep only owned ones.
  return playlists
    .filter((p) => p && p.owner?.id === spotifyUserId)
    .map((p) => ({ id: p.id, name: p.name, trackCount: p.tracks?.total ?? 0, image: p.images?.[0]?.url ?? null }));
}

/** Cached owned playlists; pass `refresh` to bust the cache first. */
export async function getPlaylists(spotifyUserId: string, refresh = false): Promise<Playlist[]> {
  if (refresh) await cache.del(keys.playlists(spotifyUserId));
  return cache.wrap(keys.playlists(spotifyUserId), TTL.playlists, () => fetchPlaylists(spotifyUserId));
}

/** Returns an existing owned playlist by exact name, or creates a private one. */
export async function findOrCreatePlaylist(spotifyUserId: string, name: string): Promise<Playlist> {
  const existing = (await getPlaylists(spotifyUserId)).find((p) => p.name === name);
  if (existing) return existing;
  const created = await spotifyFetch<any>(spotifyUserId, `/users/${encodeURIComponent(spotifyUserId)}/playlists`, {
    method: "POST",
    body: JSON.stringify({ name, public: false, description: "Generata da TracksByPopularity" }),
  });
  await cache.del(keys.playlists(spotifyUserId));
  return { id: created.id, name: created.name, trackCount: 0, image: null };
}

/** Ordered non-local track URIs currently in a playlist. */
export async function getPlaylistTrackUris(spotifyUserId: string, playlistId: string): Promise<string[]> {
  const items = await paginate<any>(
    spotifyUserId,
    `/playlists/${playlistId}/tracks?limit=100&fields=items(track(uri,is_local)),next`,
  );
  return items.map((item) => item.track).filter((track) => track?.uri && !track.is_local).map((track) => track.uri);
}

/** Splits an array into fixed-size batches (used for Spotify's 100-uri limit). */
export function chunk<T>(items: T[], size: number): T[][] {
  const chunks: T[][] = [];
  for (let i = 0; i < items.length; i += size) chunks.push(items.slice(i, i + size));
  return chunks;
}

/** Empties the playlist, then adds uris in batches of 100 (Spotify limit), preserving order. */
export async function replacePlaylistTracks(spotifyUserId: string, playlistId: string, uris: string[]) {
  await spotifyFetch(spotifyUserId, `/playlists/${playlistId}/tracks`, { method: "PUT", body: JSON.stringify({ uris: [] }) });
  for (const batch of chunk(uris, 100)) {
    await spotifyFetch(spotifyUserId, `/playlists/${playlistId}/tracks`, { method: "POST", body: JSON.stringify({ uris: batch }) });
  }
  await cache.del(keys.playlists(spotifyUserId));
}
