import { Elysia } from "elysia";
import { ApiResponse } from "../lib/response";
import { session } from "../plugins/session";
import { getPlaylists, getProfile, getSavedTracks } from "../services/library";
import { isManagedPlaylist } from "../services/organize";

export const dashboardRoutes = new Elysia({ prefix: "/api/dashboard", detail: { tags: ["Dashboard"] } })
  .use(session)
  .get(
    "/",
    async ({ spotifyUserId }) => {
      const [profile, tracks, playlists] = await Promise.all([
        getProfile(spotifyUserId),
        getSavedTracks(spotifyUserId),
        getPlaylists(spotifyUserId),
      ]);
      return ApiResponse.Ok({
        spotifyUserId,
        profile,
        savedTracks: tracks.length,
        playlists: playlists.length,
        managedPlaylists: playlists.filter((p) => isManagedPlaylist(p.name)).length,
      });
    },
    { requireSpotify: true },
  );
