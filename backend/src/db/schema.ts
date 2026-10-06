import { index, pgTable, serial, text, timestamp, uniqueIndex, uuid, varchar } from "drizzle-orm/pg-core";

/** Shared created/updated timestamps; `updatedAt` is refreshed on row update. */
const timestamps = {
  createdAt: timestamp("created_at", { withTimezone: true }).notNull().defaultNow(),
  updatedAt: timestamp("updated_at", { withTimezone: true })
    .notNull()
    .defaultNow()
    .$onUpdate(() => new Date()),
};

/** Local accounts (email + argon2id password hash). */
export const users = pgTable(
  "users",
  {
    id: uuid("id").primaryKey().defaultRandom(),
    email: varchar("email", { length: 255 }).notNull(),
    passwordHash: varchar("password_hash", { length: 255 }).notNull(),
    ...timestamps,
  },
  (table) => [uniqueIndex("users_email_idx").on(table.email)],
);

/**
 * One-to-one link between a local user and a Spotify account.
 * `accessToken` / `refreshToken` are AES-GCM encrypted (durable fallback when Redis is cold).
 * Cascades when the user is deleted.
 */
export const spotifyLinks = pgTable("spotify_links", {
  id: uuid("id").primaryKey().defaultRandom(),
  userId: uuid("user_id")
    .notNull()
    .unique()
    .references(() => users.id, { onDelete: "cascade" }),
  spotifyUserId: varchar("spotify_user_id", { length: 255 }).notNull().unique(),
  accessToken: text("access_token").notNull(),
  refreshToken: text("refresh_token").notNull(),
  ...timestamps,
});

/**
 * Playlist content backup taken before mutations (sort/split/restore).
 * `userId` is set when the Spotify account is linked; null for Spotify-only sessions.
 * On user delete, `userId` is set null so snapshots remain keyed by `spotifyUserId`.
 */
export const playlistSnapshots = pgTable(
  "playlist_snapshots",
  {
    id: uuid("id").primaryKey().defaultRandom(),
    userId: uuid("user_id").references(() => users.id, { onDelete: "set null" }),
    spotifyUserId: varchar("spotify_user_id", { length: 255 }).notNull(),
    playlistId: varchar("playlist_id", { length: 255 }).notNull(),
    playlistName: varchar("playlist_name", { length: 255 }).notNull(),
    operationType: varchar("operation_type", { length: 50 }).notNull(),
    createdAt: timestamp("created_at", { withTimezone: true }).notNull().defaultNow(),
  },
  (table) => [
    index("playlist_snapshots_spotify_user_idx").on(table.spotifyUserId),
    index("playlist_snapshots_created_at_idx").on(table.createdAt),
  ],
);

/** Ordered track URIs belonging to a snapshot; cascade-deleted with the parent row. */
export const snapshotTracks = pgTable(
  "snapshot_tracks",
  {
    id: serial("id").primaryKey(),
    snapshotId: uuid("snapshot_id")
      .notNull()
      .references(() => playlistSnapshots.id, { onDelete: "cascade" }),
    trackUri: varchar("track_uri", { length: 255 }).notNull(),
  },
  (table) => [index("snapshot_tracks_snapshot_idx").on(table.snapshotId)],
);
