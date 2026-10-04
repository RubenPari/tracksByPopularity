import { expect, test } from "bun:test";
import { filterArtists } from "./lib";

const artists = [
  { name: "Muse", popularity: 80, trackCount: 3 },
  { name: "Daft Punk", popularity: 85, trackCount: 10 },
  { name: "Mumford & Sons", popularity: 70, trackCount: 3 },
];

test("filters by name case-insensitively", () => {
  expect(filterArtists(artists, " mu ", "tracks").map((a) => a.name)).toEqual(["Mumford & Sons", "Muse"]);
});

test("sorts by saved tracks or popularity", () => {
  expect(filterArtists(artists, "", "tracks")[0]!.name).toBe("Daft Punk");
  expect(filterArtists(artists, "", "popularity").map((a) => a.popularity)).toEqual([85, 80, 70]);
});
