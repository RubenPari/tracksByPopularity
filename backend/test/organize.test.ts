import { describe, expect, test } from "bun:test";
import type { Track } from "../src/services/library";
import { previewPopularity, sortByPopularity, tracksForPopularityRange, urisForPopularityRange } from "../src/services/organize";

const track = (popularity: number, id = `t${popularity}`): Track => ({
  id,
  uri: `spotify:track:${id}`,
  name: id,
  popularity,
  artists: [],
});

describe("popularity sync/preview contract", () => {
  test("urisForPopularityRange matches tracksForPopularityRange order", () => {
    const tracks = [track(10), track(55), track(50), track(90)];
    const sorted = tracksForPopularityRange(tracks, "medium");
    expect(urisForPopularityRange(tracks, "medium")).toEqual(sorted.map((t) => t.uri));
    expect(sorted.map((t) => t.popularity)).toEqual([55, 50]);
  });

  test("sortByPopularity and previewPopularity are both wired to the shared pipeline", () => {
    // Guard against regressions that call filter without sort in one path only.
    expect(sortByPopularity.toString()).toContain("urisForPopularityRange");
    expect(previewPopularity.toString()).toContain("tracksForPopularityRange");
  });
});
