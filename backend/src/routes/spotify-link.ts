import { eq } from "drizzle-orm";
import { Elysia } from "elysia";
import { config } from "../config";
import { db } from "../db/client";
import { spotifyLinks } from "../db/schema";
import { cache, keys, TTL } from "../lib/cache";
import { ApiResponse } from "../lib/response";
import { cookieOptions, createSpotifySession, revokeSpotifySessions, session } from "../plugins/session";
import { authorizeUrl, deleteToken, linkSpotify } from "../spotify/tokens";
import { callbackQuery, completeOAuth, createOAuthState } from "./auth";

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
      await linkSpotify(state.userId!, spotifyUserId);
      cookie.spotify_user_id.set({ value: await createSpotifySession(spotifyUserId), maxAge: TTL.session, ...cookieOptions("lax") });
      return redirect(`${config.frontendOrigin}/?link=success`);
    },
    { query: callbackQuery },
  )
  .get(
    "/status",
    async ({ userId }) => {
      const [link] = await db.select().from(spotifyLinks).where(eq(spotifyLinks.userId, userId));
      return ApiResponse.Ok({ linked: !!link, spotifyUserId: link?.spotifyUserId ?? null });
    },
    { requireUser: true },
  )
  .post(
    "/unlink",
    async ({ userId, cookie }) => {
      const [link] = await db.delete(spotifyLinks).where(eq(spotifyLinks.userId, userId)).returning();
      if (link) {
        const id = link.spotifyUserId;
        await revokeSpotifySessions(id);
        await deleteToken(id);
        await cache.del(keys.tracks(id), keys.playlists(id), keys.artists(id));
      }
      cookie.spotify_user_id.remove();
      return ApiResponse.Ok(null, link ? "Account Spotify scollegato" : "Nessun account Spotify collegato");
    },
    { requireUser: true },
  );
