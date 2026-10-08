import { describe, expect, test } from "bun:test";
import { directSessionId } from "../src/plugins/session";

describe("directSessionId", () => {
  const id = "aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee";

  test("prefers X-Spotify-Session-Id header over cookie", () => {
    expect(
      directSessionId({ "x-spotify-session-id": id }, { spotify_session: { value: "11111111-1111-1111-1111-111111111111" } }),
    ).toBe(id);
  });

  test("falls back to spotify_session cookie", () => {
    expect(directSessionId({}, { spotify_session: { value: id } })).toBe(id);
  });

  test("rejects non-UUID values", () => {
    expect(directSessionId({ "x-spotify-session-id": "not-a-uuid" }, {})).toBeNull();
    expect(directSessionId({}, { spotify_session: { value: "someone-else" } })).toBeNull();
  });

  test("ignores legacy spotify_user_id cookie name", () => {
    expect(directSessionId({}, { spotify_user_id: { value: id } })).toBeNull();
  });
});
