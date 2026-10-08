import { afterEach, describe, expect, mock, test } from "bun:test";
import { decode, encode } from "../src/lib/cache";
import { decrypt, encrypt } from "../src/lib/crypto";
import { isUniqueViolation } from "../src/services/account";
import { chunk, type Track } from "../src/services/library";
import {
  inRange,
  isManagedPlaylist,
  POPULARITY_RANGES,
  popularityPlaylistName,
  splitByArtistRanges,
  tracksForPopularityRange,
  urisForPopularityRange,
} from "../src/services/organize";
import { spotifyFetch } from "../src/spotify/client";

const track = (popularity: number): Track => ({ id: `t${popularity}`, uri: `spotify:track:${popularity}`, name: "x", popularity, artists: [] });

describe("popularity ranges", () => {
  test("every value 0-100 falls in exactly one band", () => {
    for (let p = 0; p <= 100; p++) {
      expect(Object.values(POPULARITY_RANGES).filter((range) => inRange(track(p), range))).toHaveLength(1);
    }
  });
  test("playlist naming", () => expect(popularityPlaylistName("medium")).toBe("Popularity: 41-60"));
  test("managed playlist detection", () => {
    expect(["Popularity: 0-20", "Daft Punk more", "Muse less"].every(isManagedPlaylist)).toBe(true);
    expect(["Road trip", "Popularity", "less"].some(isManagedPlaylist)).toBe(false);
  });
  test("artist split boundaries", () => {
    const bands = splitByArtistRanges([0, 33, 34, 66, 67, 100].map(track));
    expect(bands.map((b) => b.uris.length)).toEqual([2, 2, 2]);
    expect(bands[1]!.uris).toEqual(["spotify:track:34", "spotify:track:66"]);
  });
  test("preview and sync share the same sorted uris pipeline", () => {
    const tracks = [track(10), track(55), track(50), track(90)];
    const medium = tracksForPopularityRange(tracks, "medium");
    expect(medium.map((t) => t.popularity)).toEqual([55, 50]);
    expect(urisForPopularityRange(tracks, "medium")).toEqual(["spotify:track:55", "spotify:track:50"]);
  });
});

test("isUniqueViolation detects Postgres 23505", () => {
  expect(isUniqueViolation({ code: "23505" })).toBe(true);
  expect(isUniqueViolation({ code: "23503" })).toBe(false);
  expect(isUniqueViolation(new Error("x"))).toBe(false);
});

test("chunk splits into Spotify batches of 100", () => {
  expect(chunk(Array.from({ length: 250 }, (_, i) => i), 100).map((c) => c.length)).toEqual([100, 100, 50]);
});

test("cache codec gzips only large payloads", () => {
  const small = { a: 1 };
  const large = { items: Array.from({ length: 500 }, (_, i) => `track-${i}`) };
  expect(encode(small)[0]).toBe(0);
  expect(encode(large)[0]).toBe(1);
  expect(decode<typeof small>(encode(small))).toEqual(small);
  expect(decode<typeof large>(encode(large))).toEqual(large);
});

test("encrypt/decrypt roundtrip", async () => {
  const cipher = await encrypt("refresh-token");
  expect(cipher).not.toContain("refresh-token");
  expect(await decrypt(cipher)).toBe("refresh-token");
});

describe("spotifyFetch", () => {
  const realFetch = globalThis.fetch;
  afterEach(() => {
    globalThis.fetch = realFetch;
  });

  test("retries 429 honouring Retry-After, then succeeds", async () => {
    const responses = [new Response("", { status: 429, headers: { "Retry-After": "0" } }), Response.json({ ok: true })];
    globalThis.fetch = mock(async () => responses.shift()!) as unknown as typeof fetch;
    expect(await spotifyFetch<{ ok: boolean }>("u", "/me", {}, async () => "token")).toEqual({ ok: true });
    expect(globalThis.fetch).toHaveBeenCalledTimes(2);
  });

  test("refreshes token once on 401", async () => {
    const responses = [new Response("", { status: 401 }), Response.json({ ok: true })];
    globalThis.fetch = mock(async () => responses.shift()!) as unknown as typeof fetch;
    const tokenProvider = mock(async (_id: string, force?: boolean) => (force ? "fresh" : "stale"));
    await spotifyFetch("u", "/me", {}, tokenProvider);
    expect(tokenProvider.mock.calls.map((c) => c[1])).toEqual([false, true]);
  });

  test("maps 404 to AppError", async () => {
    globalThis.fetch = mock(async () => new Response("", { status: 404 })) as unknown as typeof fetch;
    await expect(spotifyFetch("u", "/x", {}, async () => "t")).rejects.toMatchObject({ status: 404 });
  });
});
