import { Elysia } from "elysia";

/** Liveness probe — no auth, no dependencies. */
export const healthRoutes = new Elysia().get(
  "/health",
  () => ({ status: "healthy", timestamp: new Date().toISOString() }),
  { detail: { tags: ["Health"] } },
);
