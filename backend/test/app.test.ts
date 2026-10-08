import { expect, test } from "bun:test";
import { app } from "../src/app";

test("GET /health", async () => {
  const response = await app.handle(new Request("http://localhost/health"));
  expect(response.status).toBe(200);
  expect(await response.json()).toMatchObject({ status: "healthy" });
});

test("invalid body is rejected with 422 ApiResponse", async () => {
  const response = await app.handle(
    new Request("http://localhost/api/account/register", {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ email: "not-an-email", password: "x" }),
    }),
  );
  expect(response.status).toBe(422);
  expect(await response.json()).toMatchObject({ success: false, data: null, error: "VALIDATION_ERROR" });
});

test("protected route without session returns 401", async () => {
  const response = await app.handle(new Request("http://localhost/api/backup/list"));
  expect(response.status).toBe(401);
  expect(await response.json()).toMatchObject({ success: false, error: "SPOTIFY_NOT_AUTHENTICATED" });
});

test("forged X-Spotify-Session-Id header is ignored", async () => {
  const response = await app.handle(
    new Request("http://localhost/api/backup/list", { headers: { "X-Spotify-Session-Id": "someone-else" } }),
  );
  expect(response.status).toBe(401);
});

test("OAuth callback without the browser-bound state cookie is rejected", async () => {
  const response = await app.handle(new Request("http://localhost/auth/callback?code=abc&state=attacker-state"));
  expect(response.status).toBe(400);
  expect(await response.json()).toMatchObject({ error: "INVALID_OAUTH_STATE" });
});
