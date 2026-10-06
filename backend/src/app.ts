import { cors } from "@elysiajs/cors";
import { swagger } from "@elysiajs/swagger";
import { Elysia } from "elysia";
import { config } from "./config";
import { logger } from "./lib/logger";
import { ApiResponse, AppError } from "./lib/response";
import { accountRoutes } from "./routes/account";
import { authRoutes } from "./routes/auth";
import { backupRoutes } from "./routes/backup";
import { dashboardRoutes } from "./routes/dashboard";
import { healthRoutes } from "./routes/health";
import { playlistRoutes } from "./routes/playlist";
import { spotifyLinkRoutes } from "./routes/spotify-link";
import { trackRoutes } from "./routes/track";

/**
 * Elysia app: CORS, Swagger, request-id logging, global error envelope, and all route modules.
 * Cron jobs are mounted in `index.ts` so tests can import `app` without starting timers.
 */
export const app = new Elysia()
  .use(cors({ origin: config.frontendOrigin, credentials: true }))
  .use(swagger({ path: "/swagger", documentation: { info: { title: "TracksByPopularity API", version: "1.0.0" } } }))
  .derive({ as: "global" }, ({ set }) => {
    const requestId = crypto.randomUUID();
    set.headers["x-request-id"] = requestId;
    return { requestId, startedAt: performance.now() };
  })
  .onAfterResponse({ as: "global" }, ({ request, path, set, requestId, startedAt }) => {
    logger.info("request", {
      requestId,
      method: request.method,
      path,
      status: set.status,
      executionTimeMs: Math.round((performance.now() - startedAt) * 100) / 100,
    });
  })
  .onError({ as: "global" }, ({ code, error, set, request, path }) => {
    // Domain errors already carry HTTP status + machine-readable code.
    if (error instanceof AppError) {
      set.status = error.status;
      return ApiResponse.Fail(error.message, error.code);
    }
    switch (code) {
      case "VALIDATION":
        set.status = 422;
        return ApiResponse.Fail("Dati di input non validi", error.message);
      case "PARSE":
        set.status = 400;
        return ApiResponse.Fail("Corpo della richiesta non valido", "PARSE_ERROR");
      case "NOT_FOUND":
        set.status = 404;
        return ApiResponse.Fail("Risorsa non trovata", "NOT_FOUND");
    }
    const err = error as Error;
    logger.error("unhandled error", { method: request.method, path, error: err.message, stack: err.stack });
    set.status = 500;
    return ApiResponse.Fail("Errore interno del server", "INTERNAL_ERROR");
  })
  .use(healthRoutes)
  .use(authRoutes)
  .use(accountRoutes)
  .use(spotifyLinkRoutes)
  .use(trackRoutes)
  .use(playlistRoutes)
  .use(backupRoutes)
  .use(dashboardRoutes);

export type App = typeof app;
