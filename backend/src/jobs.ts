import { cron } from "@elysiajs/cron";
import { Elysia } from "elysia";
import { cache, keys, redis, TTL } from "./lib/cache";
import { logger, pruneLogs } from "./lib/logger";
import { fetchPlaylists } from "./services/library";
import { deleteSnapshotsOlderThan } from "./services/snapshot";

const SNAPSHOT_RETENTION_DAYS = 30;
const CACHE_PATTERNS = ["tracks:*", "playlists:*", "artists:*", "oauth_state:*"];

async function scanKeys(match: string): Promise<string[]> {
  const found: string[] = [];
  for await (const batch of redis.scanStream({ match, count: 200 })) found.push(...(batch as string[]));
  return found;
}

/** Wraps a job so a failure is logged and never crashes the process. */
const safe = (name: string, run: () => Promise<void>) => async () => {
  const startedAt = performance.now();
  try {
    await run();
    logger.info("cron completed", { job: name, executionTimeMs: Math.round(performance.now() - startedAt) });
  } catch (error) {
    logger.error("cron failed", { job: name, error: (error as Error).message, stack: (error as Error).stack });
  }
};

export async function snapshotCleanup() {
  const deleted = await deleteSnapshotsOlderThan(SNAPSHOT_RETENTION_DAYS);
  await pruneLogs();
  logger.info("snapshot cleanup", { deleted });
}

export async function redisMaintenance() {
  await redis.ping();
  let orphans = 0;
  for (const pattern of CACHE_PATTERNS) {
    for (const key of await scanKeys(pattern)) {
      // Cache entries are always written with a TTL; one without is orphaned.
      if ((await redis.ttl(key)) === -1) {
        await redis.del(key);
        orphans++;
      }
    }
  }
  const tokens = (await scanKeys("spotify_token:*")).length;
  logger.info("redis maintenance", { orphansRemoved: orphans, activeSpotifySessions: tokens });
}

export async function prefetchWarming() {
  for (const key of await scanKeys("playlists:*")) {
    const ttl = await redis.ttl(key);
    if (ttl < 0 || ttl > 60) continue;
    const spotifyUserId = key.slice("playlists:".length);
    if (!(await redis.exists(keys.token(spotifyUserId)))) continue;
    try {
      await cache.set(key, await fetchPlaylists(spotifyUserId), TTL.playlists);
    } catch (error) {
      logger.warn("prefetch failed", { spotifyUserId, error: (error as Error).message });
    }
  }
}

export const jobs = new Elysia({ name: "jobs" })
  .use(cron({ name: "SnapshotCleanupCron", pattern: "0 3 * * *", timezone: "UTC", run: safe("SnapshotCleanupCron", snapshotCleanup) }))
  .use(cron({ name: "RedisCacheMaintenanceCron", pattern: "*/5 * * * *", run: safe("RedisCacheMaintenanceCron", redisMaintenance) }))
  .use(cron({ name: "PrefetchWarmingCron", pattern: "*/5 * * * *", run: safe("PrefetchWarmingCron", prefetchWarming) }));
