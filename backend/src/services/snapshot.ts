import { and, asc, count, desc, eq, lt } from "drizzle-orm";
import { db } from "../db/client";
import { playlistSnapshots, snapshotTracks, spotifyLinks } from "../db/schema";
import { AppError } from "../lib/response";
import { getPlaylistTrackUris, replacePlaylistTracks, type Playlist } from "./library";

export type OperationType = "popularity_sort" | "artist_split" | "restore";

/** Persists the current playlist content. Must run before any mutation (snapshot-before-mutation). */
export async function createSnapshot(spotifyUserId: string, playlist: Pick<Playlist, "id" | "name">, operationType: OperationType) {
  const uris = await getPlaylistTrackUris(spotifyUserId, playlist.id);
  const [link] = await db.select({ userId: spotifyLinks.userId }).from(spotifyLinks).where(eq(spotifyLinks.spotifyUserId, spotifyUserId));
  return db.transaction(async (tx) => {
    const [snapshot] = await tx
      .insert(playlistSnapshots)
      .values({ userId: link?.userId ?? null, spotifyUserId, playlistId: playlist.id, playlistName: playlist.name, operationType })
      .returning({ id: playlistSnapshots.id });
    if (uris.length) await tx.insert(snapshotTracks).values(uris.map((trackUri) => ({ snapshotId: snapshot!.id, trackUri })));
    return snapshot!.id;
  });
}

export function listSnapshots(spotifyUserId: string) {
  return db
    .select({
      id: playlistSnapshots.id,
      playlistId: playlistSnapshots.playlistId,
      playlistName: playlistSnapshots.playlistName,
      operationType: playlistSnapshots.operationType,
      createdAt: playlistSnapshots.createdAt,
      trackCount: count(snapshotTracks.id),
    })
    .from(playlistSnapshots)
    .leftJoin(snapshotTracks, eq(snapshotTracks.snapshotId, playlistSnapshots.id))
    .where(eq(playlistSnapshots.spotifyUserId, spotifyUserId))
    .groupBy(playlistSnapshots.id)
    .orderBy(desc(playlistSnapshots.createdAt));
}

async function getOwnedSnapshot(spotifyUserId: string, snapshotId: string) {
  const [snapshot] = await db
    .select()
    .from(playlistSnapshots)
    .where(and(eq(playlistSnapshots.id, snapshotId), eq(playlistSnapshots.spotifyUserId, spotifyUserId)));
  if (!snapshot) throw new AppError(404, "SNAPSHOT_NOT_FOUND", "Snapshot non trovato");
  return snapshot;
}

export async function restoreSnapshot(spotifyUserId: string, snapshotId: string) {
  const snapshot = await getOwnedSnapshot(spotifyUserId, snapshotId);
  const tracks = await db
    .select({ uri: snapshotTracks.trackUri })
    .from(snapshotTracks)
    .where(eq(snapshotTracks.snapshotId, snapshot.id))
    .orderBy(asc(snapshotTracks.id));
  const playlist = { id: snapshot.playlistId, name: snapshot.playlistName };
  const safetySnapshotId = await createSnapshot(spotifyUserId, playlist, "restore");
  await replacePlaylistTracks(spotifyUserId, playlist.id, tracks.map((t) => t.uri));
  return { playlistId: playlist.id, playlistName: playlist.name, trackCount: tracks.length, safetySnapshotId };
}

export async function deleteSnapshot(spotifyUserId: string, snapshotId: string) {
  await getOwnedSnapshot(spotifyUserId, snapshotId);
  await db.delete(playlistSnapshots).where(eq(playlistSnapshots.id, snapshotId));
}

export async function deleteSnapshotsOlderThan(days: number) {
  const cutoff = new Date(Date.now() - days * 86_400_000);
  const deleted = await db.delete(playlistSnapshots).where(lt(playlistSnapshots.createdAt, cutoff)).returning({ id: playlistSnapshots.id });
  return deleted.length;
}
