import { jwt } from "@elysiajs/jwt";
import { eq } from "drizzle-orm";
import { Elysia } from "elysia";
import { config } from "../config";
import { db } from "../db/client";
import { spotifyLinks } from "../db/schema";
import { cache, keys, redis, TTL } from "../lib/cache";
import { AppError } from "../lib/response";

/** Shared cookie flags for JWT and Spotify session cookies. */
export const cookieOptions = (sameSite: "strict" | "lax" = "strict") => ({
  httpOnly: true,
  sameSite,
  secure: config.isProduction,
  path: "/",
});

const UUID = /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i;

/** Opaque, revocable server-side session: `spotify_session:{uuid}` -> spotifyUserId. */
export async function createSpotifySession(spotifyUserId: string) {
  const sessionId = crypto.randomUUID();
  // Track membership so logout/unlink can revoke every device at once.
  await redis
    .multi()
    .set(keys.session(sessionId), spotifyUserId, "EX", TTL.session)
    .sadd(keys.userSessions(spotifyUserId), sessionId)
    .expire(keys.userSessions(spotifyUserId), TTL.session)
    .exec();
  return sessionId;
}

/** Revokes every session of a Spotify user (all devices), e.g. on logout or unlink. */
export async function revokeSpotifySessions(spotifyUserId: string) {
  const sessionIds = await redis.smembers(keys.userSessions(spotifyUserId));
  await cache.del(keys.userSessions(spotifyUserId), ...sessionIds.map(keys.session));
}

/** Deletes a single session key (does not remove it from the user's session set). */
export async function deleteSpotifySession(sessionId: string | null) {
  if (sessionId) await cache.del(keys.session(sessionId));
}

/** Session id sent by the client: `X-Spotify-User-Id` header takes priority over the cookie. */
export function directSessionId(headers: Record<string, string | undefined>, cookie: Record<string, { value?: unknown }>) {
  const id = headers["x-spotify-user-id"] ?? (cookie.spotify_user_id?.value as string | undefined);
  return id && UUID.test(id) ? id : null;
}

/** Spotify user of a direct (header/cookie) session, ignoring any account link. */
export async function directSpotifyUserId(headers: Record<string, string | undefined>, cookie: Record<string, { value?: unknown }>) {
  const sessionId = directSessionId(headers, cookie);
  return sessionId ? redis.get(keys.session(sessionId)) : null;
}

/**
 * Global JWT + Spotify session plugin.
 * Exposes `resolveUserId` / `resolveSpotifyUserId` and `requireUser` / `requireSpotify` macros.
 */
export const session = new Elysia({ name: "session" })
  .use(jwt({ name: "jwt", secret: config.jwtSecret, exp: "7d" }))
  .derive({ as: "global" }, ({ jwt, cookie, headers }) => {
    /** Local account id from `Authorization: Bearer` or the `access_token` cookie. */
    const resolveUserId = async (): Promise<string | null> => {
      const bearer = headers.authorization?.match(/^Bearer (.+)$/)?.[1];
      const token = bearer ?? (cookie.access_token?.value as string | undefined);
      if (!token) return null;
      const payload = await jwt.verify(token);
      return payload && typeof payload.sub === "string" ? payload.sub : null;
    };
    /** Spotify session: header > cookie > Spotify account linked to the JWT user. */
    const resolveSpotifyUserId = async (): Promise<string | null> => {
      const direct = await directSpotifyUserId(headers, cookie);
      if (direct) return direct;
      const userId = await resolveUserId();
      if (!userId) return null;
      const [link] = await db.select().from(spotifyLinks).where(eq(spotifyLinks.userId, userId));
      return link?.spotifyUserId ?? null;
    };
    return { resolveUserId, resolveSpotifyUserId };
  })
  .macro({
    /** Guard: requires a valid local JWT user. */
    requireUser: {
      async resolve({ resolveUserId }) {
        const userId = await resolveUserId();
        if (!userId) throw new AppError(401, "UNAUTHORIZED", "Autenticazione richiesta");
        return { userId };
      },
    },
    /** Guard: requires a resolvable Spotify user (direct session or linked account). */
    requireSpotify: {
      async resolve({ resolveSpotifyUserId }) {
        const spotifyUserId = await resolveSpotifyUserId();
        if (!spotifyUserId) throw new AppError(401, "SPOTIFY_NOT_AUTHENTICATED", "Sessione Spotify richiesta");
        return { spotifyUserId };
      },
    },
  });
