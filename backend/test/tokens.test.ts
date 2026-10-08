import { describe, expect, test } from "bun:test";
import { decrypt, encrypt } from "../src/lib/crypto";
import type { StoredToken } from "../src/spotify/tokens";

describe("token storage codec", () => {
  test("StoredToken survives encrypt/decrypt roundtrip used by saveToken/loadToken", async () => {
    const token: StoredToken = {
      accessToken: "access",
      refreshToken: "refresh",
      expiresAt: Date.now() + 3600_000,
    };
    const cipher = await encrypt(JSON.stringify(token));
    expect(cipher).not.toContain("access");
    expect(JSON.parse(await decrypt(cipher)) as StoredToken).toEqual(token);
  });

  test("Postgres fallback shape forces refresh via expiresAt 0", async () => {
    const link = {
      accessToken: await encrypt("pg-access"),
      refreshToken: await encrypt("pg-refresh"),
    };
    const fallback: StoredToken = {
      accessToken: await decrypt(link.accessToken),
      refreshToken: await decrypt(link.refreshToken),
      expiresAt: 0,
    };
    expect(fallback.expiresAt).toBe(0);
    expect(fallback.accessToken).toBe("pg-access");
    expect(fallback.refreshToken).toBe("pg-refresh");
  });
});
