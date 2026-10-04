import Redis from "ioredis";
import { config } from "../config";
import { logger } from "./logger";

export const redis = new Redis({
  ...config.redis,
  lazyConnect: true,
  maxRetriesPerRequest: 3,
  retryStrategy: (attempt) => Math.min(2 ** attempt * 100, 5_000),
});
redis.on("error", (error) => logger.warn("redis error", { error: error.message }));

const GZIP_THRESHOLD = 2048;
const RAW = 0x00;
const GZIP = 0x01;

/** Serializes to a buffer with a 1-byte marker; payloads over 2KB are gzipped. */
export function encode(value: unknown): Buffer {
  const json = Buffer.from(JSON.stringify(value));
  if (json.length <= GZIP_THRESHOLD) return Buffer.concat([Buffer.from([RAW]), json]);
  return Buffer.concat([Buffer.from([GZIP]), Buffer.from(Bun.gzipSync(new Uint8Array(json)))]);
}

export function decode<T>(buffer: Buffer): T {
  const body = buffer.subarray(1);
  const json = buffer[0] === GZIP ? Buffer.from(Bun.gunzipSync(new Uint8Array(body))) : body;
  return JSON.parse(json.toString()) as T;
}

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

export const cache = {
  async get<T>(key: string): Promise<T | null> {
    const buffer = await withRetry(() => redis.getBuffer(key));
    return buffer ? decode<T>(buffer) : null;
  },
  async set(key: string, value: unknown, ttlSeconds?: number) {
    const payload = encode(value);
    await withRetry(() => (ttlSeconds ? redis.set(key, payload, "EX", ttlSeconds) : redis.set(key, payload)));
  },
  async del(...keys: string[]) {
    if (keys.length) await withRetry(() => redis.del(...keys));
  },
  /** Returns cached value or computes, stores and returns it. */
  async wrap<T>(key: string, ttlSeconds: number, compute: () => Promise<T>): Promise<T> {
    const cached = await cache.get<T>(key);
    if (cached !== null) return cached;
    const value = await compute();
    await cache.set(key, value, ttlSeconds);
    return value;
  },
};

export const keys = {
  tracks: (spotifyUserId: string) => `tracks:${spotifyUserId}`,
  playlists: (spotifyUserId: string) => `playlists:${spotifyUserId}`,
  artists: (spotifyUserId: string) => `artists:${spotifyUserId}`,
  profile: (spotifyUserId: string) => `profile:${spotifyUserId}`,
  token: (spotifyUserId: string) => `spotify_token:${spotifyUserId}`,
  oauthState: (state: string) => `oauth_state:${state}`,
  session: (sessionId: string) => `spotify_session:${sessionId}`,
  userSessions: (spotifyUserId: string) => `spotify_sessions:${spotifyUserId}`,
};

export const TTL = { profile: 15 * 60, playlists: 15 * 60, tracks: 30 * 60, artists: 30 * 60, oauthState: 10 * 60, session: 30 * 86_400 };
