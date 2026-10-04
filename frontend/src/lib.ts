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

export const POPULARITY_RANGES = [
  { key: "less", label: "0-20", description: "Gemme nascoste" },
  { key: "less-medium", label: "21-40", description: "Poco conosciuti" },
  { key: "medium", label: "41-60", description: "Via di mezzo" },
  { key: "more-medium", label: "61-80", description: "Molto ascoltati" },
  { key: "more", label: "81-100", description: "Hit globali" },
] as const;
export type PopularityRange = (typeof POPULARITY_RANGES)[number]["key"];

export const OPERATION_LABELS: Record<string, string> = {
  popularity_sort: "Ordinamento popolarità",
  artist_split: "Ripartizione artista",
  restore: "Ripristino",
};

export const formatDate = (value: string | Date) =>
  new Intl.DateTimeFormat("it-IT", { dateStyle: "medium", timeStyle: "short" }).format(new Date(value));
