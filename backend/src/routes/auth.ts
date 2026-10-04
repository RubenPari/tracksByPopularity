import { Elysia, t } from "elysia";
import { config } from "../config";
import { cache, keys, TTL } from "../lib/cache";
import { sign } from "../lib/crypto";
import { ApiResponse, AppError } from "../lib/response";
import { cookieOptions, session } from "../plugins/session";
import { authorizeUrl, deleteToken, exchangeCode, fetchSpotifyUserId, loadToken, saveToken } from "../spotify/tokens";

export type OAuthState = { purpose: "login" | "link"; userId?: string };

export async function createOAuthState(state: OAuthState) {
  const id = crypto.randomUUID();
  await cache.set(keys.oauthState(id), state, TTL.oauthState);
  return id;
}

/** Validates (single use) the OAuth state, exchanges the code and stores the tokens. */
export async function completeOAuth(stateId: string, code: string, purpose: OAuthState["purpose"], redirectUri: string) {
  const state = await cache.get<OAuthState>(keys.oauthState(stateId));
  await cache.del(keys.oauthState(stateId));
  if (!state || state.purpose !== purpose) throw new AppError(400, "INVALID_OAUTH_STATE", "Stato OAuth non valido o scaduto");
  const token = await exchangeCode(code, redirectUri);
  const spotifyUserId = await fetchSpotifyUserId(token.accessToken);
  await saveToken(spotifyUserId, token);
  return { state, spotifyUserId };
}

export const callbackQuery = t.Object({
  code: t.Optional(t.String()),
  state: t.String(),
  error: t.Optional(t.String()),
});

export const authRoutes = new Elysia({ prefix: "/auth", detail: { tags: ["Auth Spotify"] } })
  .use(session)
  .get("/login", async () => {
    const state = await createOAuthState({ purpose: "login" });
    return ApiResponse.Ok({ url: authorizeUrl(state, config.spotify.redirectUri) });
  })
  .get(
    "/callback",
    async ({ query, cookie, redirect }) => {
      if (query.error || !query.code) return redirect(`${config.frontendOrigin}/?auth=denied`);
      const { spotifyUserId } = await completeOAuth(query.state, query.code, "login", config.spotify.redirectUri);
      cookie.spotify_user_id.set({ value: sign(spotifyUserId), maxAge: 30 * 86_400, ...cookieOptions("lax") });
      return redirect(`${config.frontendOrigin}/?auth=success`);
    },
    { query: callbackQuery },
  )
  .get("/is-auth", async ({ resolveSpotifyUserId }) => {
    const spotifyUserId = await resolveSpotifyUserId();
    const authenticated = !!spotifyUserId && !!(await loadToken(spotifyUserId));
    return ApiResponse.Ok({
      authenticated,
      spotifyUserId: authenticated ? spotifyUserId : null,
      // Signed value for clients that send the X-Spotify-User-Id header instead of the cookie.
      sessionToken: authenticated ? sign(spotifyUserId!) : null,
    });
  })
  .post(
    "/logout",
    async ({ spotifyUserId, cookie }) => {
      await deleteToken(spotifyUserId);
      cookie.spotify_user_id.remove();
      return ApiResponse.Ok(null, "Logout Spotify effettuato");
    },
    { requireSpotify: true },
  );
