import Redis from "ioredis";
import { config } from "../config";
import { logger } from "./logger";

/** Shared Redis client: lazy connect, exponential reconnect backoff. */
export const redis = new Redis({
  ...config.redis,
  lazyConnect: true,
  maxRetriesPerRequest: 3,
  retryStrategy: (attempt) => Math.min(2 ** attempt * 100, 5_000),
});
redis.on("error", (error) => logger.warn("redis error", { error: error.message }));

/** Gzip payloads larger than this (bytes of JSON); first byte is RAW/GZIP marker. */
const GZIP_THRESHOLD = 2048;
const RAW = 0x00;
const GZIP = 0x01;

/**
 * Encodes a value to a buffer.
 * @param value - The value to encode.
 * @returns The encoded value.
 */
export function encode(value: unknown): Buffer {
  const json = Buffer.from(JSON.stringify(value));
  if (json.length <= GZIP_THRESHOLD) return Buffer.concat([Buffer.from([RAW]), json]);
  return Buffer.concat([Buffer.from([GZIP]), Buffer.from(Bun.gzipSync(new Uint8Array(json)))]);
}

/**
 * Decodes a buffer to a value.
 * @param buffer - The buffer to decode.
 * @returns The decoded value.
 */
export function decode<T>(buffer: Buffer): T {
  const body = buffer.subarray(1);
  const json = buffer[0] === GZIP ? Buffer.from(Bun.gunzipSync(new Uint8Array(body))) : body;
  return JSON.parse(json.toString()) as T;
}

/**
 * Retries an operation up to a certain number of attempts.
 * @param operation - The operation to retry.
 * @param attempts - The number of attempts to retry.
 * @returns The result of the operation.
 */
async function withRetry<T>(operation: () => Promise<T>, attempts = 3): Promise<T> {
  for (let attempt = 1; ; attempt++) {
    try {
      return await operation();
    } catch (error) {
      if (attempt >= attempts) throw error;
      await Bun.sleep(2 ** attempt * 100);
    }
  }
}

/**
 * The cache object.
 */
export const cache = {
  /**
   * Gets a value from the cache.
   * @param key - The key to get.
   * @returns The value or null if not found.
   */
  async get<T>(key: string): Promise<T | null> {
    const buffer = await withRetry(() => redis.getBuffer(key));
    return buffer ? decode<T>(buffer) : null;
  },
  /**
   * Sets a value in the cache.
   * @param key - The key to set.
   * @param value - The value to set.
   * @param ttlSeconds - The time to live in seconds.
   */
  async set(key: string, value: unknown, ttlSeconds?: number) {
    const payload = encode(value);
    await withRetry(() => (ttlSeconds ? redis.set(key, payload, "EX", ttlSeconds) : redis.set(key, payload)));
  },
  /**
   * Deletes a value from the cache.
   * @param keys - The keys to delete.
   */
  async del(...keys: string[]) {
    if (keys.length) await withRetry(() => redis.del(...keys));
  },
  /**
   * Wraps a function to cache the result.
   * @param key - The key to cache the result.
   * @param ttlSeconds - The time to live in seconds.
   * @param compute - The function to compute the result.
   * @returns The result of the function.
   */
  async wrap<T>(key: string, ttlSeconds: number, compute: () => Promise<T>): Promise<T> {
    const cached = await cache.get<T>(key);
    if (cached !== null) return cached;
    const value = await compute();
    await cache.set(key, value, ttlSeconds);
    return value;
  },
};

/**
 * The cache keys.
 */
export const keys = {
  /**
   * The key for the tracks.
   * @param spotifyUserId - The Spotify user ID.
   * @returns The key for the tracks.
   */
  tracks: (spotifyUserId: string) => `tracks:${spotifyUserId}`,
  /**
   * The key for the playlists.
   * @param spotifyUserId - The Spotify user ID.
   * @returns The key for the playlists.
   */
  playlists: (spotifyUserId: string) => `playlists:${spotifyUserId}`,
  /**
   * The key for the artists.
   * @param spotifyUserId - The Spotify user ID.
   * @returns The key for the artists.
   */
  artists: (spotifyUserId: string) => `artists:${spotifyUserId}`,
  /**
   * The key for the profile.
   * @param spotifyUserId - The Spotify user ID.
   * @returns The key for the profile.
   */
  profile: (spotifyUserId: string) => `profile:${spotifyUserId}`,
  /**
   * The key for the token.
   * @param spotifyUserId - The Spotify user ID.
   * @returns The key for the token.
   */
  token: (spotifyUserId: string) => `spotify_token:${spotifyUserId}`,
  /**
   * The key for the OAuth state.
   * @param state - The OAuth state.
   * @returns The key for the OAuth state.
   */
  oauthState: (state: string) => `oauth_state:${state}`,
  /**
   * The key for the session.
   * @param sessionId - The session ID.
   * @returns The key for the session.
   */
  session: (sessionId: string) => `spotify_session:${sessionId}`,
  /**
   * The key for the user sessions.
   * @param spotifyUserId - The Spotify user ID.
   * @returns The key for the user sessions.
   */
  userSessions: (spotifyUserId: string) => `spotify_sessions:${spotifyUserId}`,
};

/**
 * The time to live for the cache.
 */
export const TTL = {
  profile: 15 * 60,
  playlists: 15 * 60,
  tracks: 30 * 60,
  artists: 30 * 60,
  oauthState: 10 * 60,
  session: 30 * 86_400,
};
