import { Elysia } from "elysia";
import { withEtag } from "../lib/etag";
import { ApiResponse } from "../lib/response";
import { session } from "../plugins/session";
import { getPlaylists } from "../services/library";

/** Owned playlists: cached list with ETag, and forced cache refresh. */
export const playlistRoutes = new Elysia({ prefix: "/api/playlist", detail: { tags: ["Playlist"] } })
  .use(session)
  .get("/all", async (ctx) => withEtag(ctx, await getPlaylists(ctx.spotifyUserId)), {
    requireSpotify: true,
  })
  .post("/refresh", async ({ spotifyUserId }) => ApiResponse.Ok(await getPlaylists(spotifyUserId, true)), {
    requireSpotify: true,
  });
