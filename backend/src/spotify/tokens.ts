import { eq } from "drizzle-orm";
import { config } from "../config";
import { db } from "../db/client";
import { spotifyLinks } from "../db/schema";
import { cache, keys, redis } from "../lib/cache";
import { decrypt, encrypt } from "../lib/crypto";
import { AppError } from "../lib/response";

export const SCOPES = [
  "user-read-email",
  "user-read-private",
  "user-library-read",
  "user-library-modify",
  "user-top-read",
  "playlist-modify-private",
  "playlist-modify-public",
  "user-follow-read",
];

export type StoredToken = { accessToken: string; refreshToken: string; expiresAt: number };

export function authorizeUrl(state: string, redirectUri: string) {
  const params = new URLSearchParams({
    response_type: "code",
    client_id: config.spotify.clientId,
    scope: SCOPES.join(" "),
    redirect_uri: redirectUri,
    state,
  });
  return `https://accounts.spotify.com/authorize?${params}`;
}

async function tokenRequest(body: Record<string, string>) {
  const response = await fetch("https://accounts.spotify.com/api/token", {
    method: "POST",
    headers: {
      "Content-Type": "application/x-www-form-urlencoded",
      Authorization: `Basic ${btoa(`${config.spotify.clientId}:${config.spotify.clientSecret}`)}`,
    },
    body: new URLSearchParams(body),
  });
  if (!response.ok) throw new AppError(401, "SPOTIFY_TOKEN_ERROR", "Autenticazione Spotify fallita");
  return (await response.json()) as { access_token: string; refresh_token?: string; expires_in: number };
}

export async function exchangeCode(code: string, redirectUri: string): Promise<StoredToken> {
  const data = await tokenRequest({ grant_type: "authorization_code", code, redirect_uri: redirectUri });
  return {
    accessToken: data.access_token,
    refreshToken: data.refresh_token ?? "",
    expiresAt: Date.now() + data.expires_in * 1000,
  };
}

/** Fetches the Spotify profile id for a fresh access token. */
export async function fetchSpotifyUserId(accessToken: string): Promise<string> {
  const response = await fetch("https://api.spotify.com/v1/me", { headers: { Authorization: `Bearer ${accessToken}` } });
  if (!response.ok) throw new AppError(401, "SPOTIFY_PROFILE_ERROR", "Impossibile leggere il profilo Spotify");
  return ((await response.json()) as { id: string }).id;
}

export async function saveToken(spotifyUserId: string, token: StoredToken) {
  await redis.set(keys.token(spotifyUserId), await encrypt(JSON.stringify(token)));
  // Keep the linked account's offline copy in sync.
  await db
    .update(spotifyLinks)
    .set({ accessToken: await encrypt(token.accessToken), refreshToken: await encrypt(token.refreshToken) })
    .where(eq(spotifyLinks.spotifyUserId, spotifyUserId));
}

export async function loadToken(spotifyUserId: string): Promise<StoredToken | null> {
  const stored = await redis.get(keys.token(spotifyUserId));
  if (stored) return JSON.parse(await decrypt(stored)) as StoredToken;
  // Fallback: linked local account keeps an encrypted refresh token in Postgres.
  const [link] = await db.select().from(spotifyLinks).where(eq(spotifyLinks.spotifyUserId, spotifyUserId));
  if (!link) return null;
  const token = { accessToken: await decrypt(link.accessToken), refreshToken: await decrypt(link.refreshToken), expiresAt: 0 };
  await redis.set(keys.token(spotifyUserId), await encrypt(JSON.stringify(token)));
  return token;
}

export async function deleteToken(spotifyUserId: string) {
  await cache.del(keys.token(spotifyUserId));
}

/** Returns a valid access token, refreshing it when it is about to expire. */
export async function getAccessToken(spotifyUserId: string, forceRefresh = false): Promise<string> {
  const token = await loadToken(spotifyUserId);
  if (!token) throw new AppError(401, "SPOTIFY_NOT_AUTHENTICATED", "Sessione Spotify assente o scaduta");
  if (!forceRefresh && token.expiresAt - 60_000 > Date.now()) return token.accessToken;
  const data = await tokenRequest({ grant_type: "refresh_token", refresh_token: token.refreshToken });
  const refreshed = {
    accessToken: data.access_token,
    refreshToken: data.refresh_token ?? token.refreshToken,
    expiresAt: Date.now() + data.expires_in * 1000,
  };
  await saveToken(spotifyUserId, refreshed);
  return refreshed.accessToken;
}

/** Associates a Spotify account (with its current tokens) to a local user. */
export async function linkSpotify(userId: string, spotifyUserId: string) {
  const token = await loadToken(spotifyUserId);
  if (!token) throw new AppError(401, "SPOTIFY_NOT_AUTHENTICATED", "Sessione Spotify assente o scaduta");
  const [existing] = await db.select().from(spotifyLinks).where(eq(spotifyLinks.spotifyUserId, spotifyUserId));
  if (existing && existing.userId !== userId) {
    throw new AppError(409, "SPOTIFY_ALREADY_LINKED", "Account Spotify già collegato a un altro utente");
  }
  const values = {
    spotifyUserId,
    accessToken: await encrypt(token.accessToken),
    refreshToken: await encrypt(token.refreshToken),
  };
  await db
    .insert(spotifyLinks)
    .values({ userId, ...values })
    .onConflictDoUpdate({ target: spotifyLinks.userId, set: values });
}
