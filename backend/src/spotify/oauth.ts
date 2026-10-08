import { cache, keys, TTL } from "../lib/cache";
import { AppError } from "../lib/response";
import { cookieOptions } from "../plugins/session";
import { exchangeCode, fetchSpotifyUserId, saveToken } from "./tokens";

/** Purpose of an OAuth round-trip: login creates a Spotify session; link attaches it to a local user. */
export type OAuthState = { purpose: "login" | "link"; userId?: string };

type Cookie = { value?: unknown; set(options: Record<string, unknown>): unknown; remove(): unknown };

/** Creates a single-use OAuth state, bound to the initiating browser via the `oauth_state` cookie. */
export async function createOAuthState(state: OAuthState, cookie: Cookie) {
  const id = crypto.randomUUID();
  await cache.set(keys.oauthState(id), state, TTL.oauthState);
  // Lax so the cookie survives the cross-site redirect back from Spotify.
  cookie.set({ value: id, maxAge: TTL.oauthState, ...cookieOptions("lax") });
  return id;
}

/** Validates the OAuth state against the browser cookie, exchanges the code and stores the tokens. */
export async function completeOAuth(
  stateId: string,
  cookie: Cookie,
  code: string,
  purpose: OAuthState["purpose"],
  redirectUri: string,
) {
  const boundToBrowser = cookie.value === stateId;
  cookie.remove();
  if (!boundToBrowser) throw new AppError(400, "INVALID_OAUTH_STATE", "Stato OAuth non valido o scaduto");
  const state = await cache.get<OAuthState>(keys.oauthState(stateId));
  await cache.del(keys.oauthState(stateId));
  if (!state || state.purpose !== purpose) {
    throw new AppError(400, "INVALID_OAUTH_STATE", "Stato OAuth non valido o scaduto");
  }
  const token = await exchangeCode(code, redirectUri);
  const spotifyUserId = await fetchSpotifyUserId(token.accessToken);
  await saveToken(spotifyUserId, token);
  return { state, spotifyUserId };
}
