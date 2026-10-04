import { Elysia, t } from "elysia";
import { withEtag } from "../lib/etag";
import { ApiResponse } from "../lib/response";
import { session } from "../plugins/session";
import { listLibraryArtists, POPULARITY_RANGES, sortByPopularity, splitArtist, type PopularityRange } from "../services/organize";

export const trackRoutes = new Elysia({ prefix: "/api/track", detail: { tags: ["Tracce"] } })
  .use(session)
  .post(
    "/popularity/:range",
    async ({ params, spotifyUserId }) => ApiResponse.Ok(await sortByPopularity(spotifyUserId, params.range as PopularityRange)),
    {
      requireSpotify: true,
      params: t.Object({ range: t.UnionEnum(Object.keys(POPULARITY_RANGES) as [PopularityRange, ...PopularityRange[]]) }),
    },
  )
  .get("/artists", async ({ request, spotifyUserId }) => withEtag(request, await listLibraryArtists(spotifyUserId)), {
    requireSpotify: true,
  })
  .post("/artist", async ({ body, spotifyUserId }) => ApiResponse.Ok(await splitArtist(spotifyUserId, body.artistId)), {
    requireSpotify: true,
    body: t.Object({ artistId: t.String({ pattern: "^[A-Za-z0-9]{1,64}$" }) }),
  });
