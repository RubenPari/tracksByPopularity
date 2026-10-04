import { Elysia, t } from "elysia";
import { withEtag } from "../lib/etag";
import { ApiResponse } from "../lib/response";
import { session } from "../plugins/session";
import {
  listLibraryArtists,
  POPULARITY_RANGES,
  previewPopularity,
  sortByPopularity,
  splitArtist,
  type PopularityRange,
} from "../services/organize";

const rangeParams = t.Object({
  range: t.UnionEnum(Object.keys(POPULARITY_RANGES) as [PopularityRange, ...PopularityRange[]]),
});

export const trackRoutes = new Elysia({ prefix: "/api/track", detail: { tags: ["Tracce"] } })
  .use(session)
  .post(
    "/popularity/:range",
    async ({ params, spotifyUserId }) => ApiResponse.Ok(await sortByPopularity(spotifyUserId, params.range as PopularityRange)),
    { requireSpotify: true, params: rangeParams },
  )
  .get(
    "/popularity/:range/preview",
    async ({ params, spotifyUserId }) => ApiResponse.Ok(await previewPopularity(spotifyUserId, params.range as PopularityRange)),
    { requireSpotify: true, params: rangeParams },
  )
  .get("/artists", async (ctx) => withEtag(ctx, await listLibraryArtists(ctx.spotifyUserId)), {
    requireSpotify: true,
  })
  .post("/artist", async ({ body, spotifyUserId }) => ApiResponse.Ok(await splitArtist(spotifyUserId, body.artistId)), {
    requireSpotify: true,
    body: t.Object({ artistId: t.String({ pattern: "^[A-Za-z0-9]{1,64}$" }) }),
  });
