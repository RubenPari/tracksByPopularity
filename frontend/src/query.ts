import type { QueryClient } from "@tanstack/react-query";

/** Query keys invalidated after library mutations (sync, split, restore). */
export const LIBRARY_QUERY_KEYS = ["dashboard", "backups", "artists", "preview"] as const;

export function invalidateLibrary(queryClient: QueryClient) {
  for (const key of LIBRARY_QUERY_KEYS) {
    void queryClient.invalidateQueries({ queryKey: [key] });
  }
}
