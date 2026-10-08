import { Elysia, t } from "elysia";
import { config } from "../config";
import { ApiResponse } from "../lib/response";
import { directSessionId, session, setSpotifySessionCookie } from "../plugins/session";
import { clearSpotifyAccess } from "../services/spotify-access";
import { completeOAuth, createOAuthState } from "../spotify/oauth";
import { authorizeUrl, loadToken } from "../spotify/tokens";

export const callbackQuery = t.Object({
  code: t.Optional(t.String()),
  state: t.String(),
  error: t.Optional(t.String()),
});

/** Spotify-only login: authorize URL, OAuth callback, auth check, and logout. */
export const authRoutes = new Elysia({ prefix: "/auth", detail: { tags: ["Auth Spotify"] } })
  .use(session)
  .get("/login", async ({ cookie }) => {
    const state = await createOAuthState({ purpose: "login" }, cookie.oauth_state);
    return ApiResponse.Ok({ url: authorizeUrl(state, config.spotify.redirectUri) });
  })
  .get(
    "/callback",
    async ({ query, cookie, redirect }) => {
      if (query.error || !query.code) return redirect(`${config.frontendOrigin}/?auth=denied`);
      const { spotifyUserId } = await completeOAuth(query.state, cookie.oauth_state, query.code, "login", config.spotify.redirectUri);
      await setSpotifySessionCookie(cookie.spotify_session, spotifyUserId);
      return redirect(`${config.frontendOrigin}/?auth=success`);
    },
    { query: callbackQuery },
  )
  .get("/is-auth", async ({ resolveSpotifyUserId, headers, cookie }) => {
    const spotifyUserId = await resolveSpotifyUserId();
    const authenticated = !!spotifyUserId && !!(await loadToken(spotifyUserId));
    return ApiResponse.Ok({
      authenticated,
      spotifyUserId: authenticated ? spotifyUserId : null,
      // Session id for clients that send the X-Spotify-Session-Id header instead of the cookie.
      sessionToken: authenticated ? directSessionId(headers, cookie) : null,
    });
  })
  .post(
    "/logout",
    async ({ spotifyUserId, cookie }) => {
      await clearSpotifyAccess(spotifyUserId);
      cookie.spotify_session.remove();
      return ApiResponse.Ok(null, "Logout Spotify effettuato");
    },
    { requireSpotify: true },
  );
