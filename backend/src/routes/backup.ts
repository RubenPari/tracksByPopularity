import { Elysia, t } from "elysia";
import { ApiResponse } from "../lib/response";
import { session } from "../plugins/session";
import { deleteSnapshot, listSnapshots, restoreSnapshot } from "../services/snapshot";

const idParams = t.Object({ id: t.String({ format: "uuid" }) });

export const backupRoutes = new Elysia({ prefix: "/api/backup", detail: { tags: ["Backup"] } })
  .use(session)
  .get("/list", async ({ spotifyUserId }) => ApiResponse.Ok(await listSnapshots(spotifyUserId)), { requireSpotify: true })
  .post(
    "/restore/:id",
    async ({ params, spotifyUserId }) => ApiResponse.Ok(await restoreSnapshot(spotifyUserId, params.id), "Playlist ripristinata"),
    { requireSpotify: true, params: idParams },
  )
  .delete(
    "/:id",
    async ({ params, spotifyUserId }) => {
      await deleteSnapshot(spotifyUserId, params.id);
      return ApiResponse.Ok(null, "Snapshot eliminato");
    },
    { requireSpotify: true, params: idParams },
  );
