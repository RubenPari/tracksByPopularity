import { useEffect, useState } from "react";

export type ArtistSort = "tracks" | "popularity";
type SortableArtist = { name: string; popularity: number; trackCount: number };

/** Case-insensitive name filter + sort (desc) by saved tracks or popularity. */
export function filterArtists<T extends SortableArtist>(artists: T[], query: string, sort: ArtistSort): T[] {
  const needle = query.trim().toLowerCase();
  return artists
    .filter((artist) => artist.name.toLowerCase().includes(needle))
    .sort((a, b) => (sort === "tracks" ? b.trackCount - a.trackCount : b.popularity - a.popularity) || a.name.localeCompare(b.name));
}

export function useDebounced<T>(value: T, delayMs = 300): T {
  const [debounced, setDebounced] = useState(value);
  useEffect(() => {
    const timer = setTimeout(() => setDebounced(value), delayMs);
    return () => clearTimeout(timer);
  }, [value, delayMs]);
  return debounced;
}

/** Must stay aligned with backend `POPULARITY_RANGES` / `ARTIST_RANGES`. */
export const POPULARITY_RANGES = [
  { key: "less", min: 0, max: 20, description: "Gemme nascoste" },
  { key: "less-medium", min: 21, max: 40, description: "Poco conosciuti" },
  { key: "medium", min: 41, max: 60, description: "Via di mezzo" },
  { key: "more-medium", min: 61, max: 80, description: "Molto ascoltati" },
  { key: "more", min: 81, max: 100, description: "Hit globali" },
] as const;
export type PopularityRange = (typeof POPULARITY_RANGES)[number]["key"];

export const ARTIST_RANGES = {
  less: [0, 33],
  medium: [34, 66],
  more: [67, 100],
} as const;

export const OPERATION_LABELS: Record<string, string> = {
  popularity_sort: "Ordinamento popolarità",
  artist_split: "Ripartizione artista",
  restore: "Ripristino",
};

export const formatDate = (value: string | Date) =>
  new Intl.DateTimeFormat("it-IT", { dateStyle: "medium", timeStyle: "short" }).format(new Date(value));
