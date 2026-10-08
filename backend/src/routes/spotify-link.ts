import { Elysia } from "elysia";
import { config } from "../config";
import { ApiResponse, AppError } from "../lib/response";
import {
  deleteSpotifySession,
  directSessionId,
  session,
  setSpotifySessionCookie,
} from "../plugins/session";
import { getLinkByUserId, unlinkSpotify } from "../services/account";
import { clearSpotifyAccess } from "../services/spotify-access";
import { completeOAuth, createOAuthState } from "../spotify/oauth";
import { authorizeUrl, linkSpotify } from "../spotify/tokens";
import { callbackQuery } from "./auth";

/**
 * OAuth link flow for a logged-in local user: authorize URL, callback, status, and unlink.
 * Uses a dedicated redirect URI (`linkRedirectUri`) separate from Spotify-only login.
 */
export const spotifyLinkRoutes = new Elysia({ prefix: "/api/spotify", detail: { tags: ["Spotify Link"] } })
  .use(session)
  .get(
    "/link-url",
    async ({ userId, cookie }) => {
      const state = await createOAuthState({ purpose: "link", userId }, cookie.oauth_state);
      return ApiResponse.Ok({ url: authorizeUrl(state, config.spotify.linkRedirectUri) });
    },
    { requireUser: true },
  )
  .get(
    "/callback",
    async ({ query, cookie, redirect }) => {
      if (query.error || !query.code) return redirect(`${config.frontendOrigin}/?link=denied`);
      // access_token is SameSite=Strict and not sent on Spotify's redirect; the oauth_state cookie binds the browser.
      const { state, spotifyUserId } = await completeOAuth(
        query.state,
        cookie.oauth_state,
        query.code,
        "link",
        config.spotify.linkRedirectUri,
      );
      if (!state.userId) throw new AppError(400, "INVALID_OAUTH_STATE", "Stato OAuth non valido o scaduto");
      await linkSpotify(state.userId, spotifyUserId);
      await setSpotifySessionCookie(cookie.spotify_session, spotifyUserId);
      return redirect(`${config.frontendOrigin}/?link=success`);
    },
    { query: callbackQuery },
  )
  .get(
    "/status",
    async ({ userId }) => {
      const link = await getLinkByUserId(userId);
      return ApiResponse.Ok({ linked: !!link, spotifyUserId: link?.spotifyUserId ?? null });
    },
    { requireUser: true },
  )
  .post(
    "/unlink",
    async ({ userId, headers, cookie }) => {
      // Always drop the caller's own session, even when no link exists.
      await deleteSpotifySession(directSessionId(headers, cookie));
      const link = await unlinkSpotify(userId);
      if (link) await clearSpotifyAccess(link.spotifyUserId);
      cookie.spotify_session.remove();
      return ApiResponse.Ok(null, link ? "Account Spotify scollegato" : "Nessun account Spotify collegato");
    },
    { requireUser: true },
  );
