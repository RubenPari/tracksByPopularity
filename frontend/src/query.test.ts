import { describe, expect, test } from "bun:test";
import { LIBRARY_QUERY_KEYS } from "./query";

describe("invalidateLibrary keys", () => {
  test("covers real UI query keys and omits unused playlists", () => {
    expect([...LIBRARY_QUERY_KEYS]).toEqual(["dashboard", "backups", "artists", "preview"]);
    expect(LIBRARY_QUERY_KEYS).not.toContain("playlists");
  });
});
