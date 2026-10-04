import { Elysia } from "elysia";

export const healthRoutes = new Elysia().get(
  "/health",
  () => ({ status: "healthy", timestamp: new Date().toISOString() }),
  { detail: { tags: ["Health"] } },
);
