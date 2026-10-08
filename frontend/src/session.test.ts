import { describe, expect, test } from "bun:test";
import { ApiError } from "./api";

/** Mirrors the me queryFn error policy in session.ts without mounting React. */
async function resolveAccount(fetchMe: () => Promise<unknown>) {
  try {
    return await fetchMe();
  } catch (error) {
    if (error instanceof ApiError && (error.status === 401 || error.status === 403)) return null;
    throw error;
  }
}

describe("session me error handling", () => {
  test("401/403 become null (no local account)", async () => {
    await expect(resolveAccount(async () => {
      throw new ApiError("Autenticazione richiesta", 401);
    })).resolves.toBeNull();
    await expect(resolveAccount(async () => {
      throw new ApiError("Forbidden", 403);
    })).resolves.toBeNull();
  });

  test("other ApiErrors propagate", async () => {
    await expect(resolveAccount(async () => {
      throw new ApiError("Errore interno", 500);
    })).rejects.toMatchObject({ status: 500 });
  });

  test("successful payload is returned", async () => {
    const me = { id: "u1", email: "a@b.c", spotifyLinked: false, spotifyUserId: null };
    await expect(resolveAccount(async () => me)).resolves.toEqual(me);
  });
});
