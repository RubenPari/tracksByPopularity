import { logger } from "../lib/logger";
import { AppError } from "../lib/response";
import { getAccessToken } from "./tokens";

const API = "https://api.spotify.com/v1";
const MAX_ATTEMPTS = 5;

/**
 * Calls the Spotify Web API on behalf of a user.
 * Retries 429 (honouring Retry-After) and 5xx with exponential backoff; refreshes the token once on 401.
 */
export async function spotifyFetch<T = unknown>(
  spotifyUserId: string,
  pathOrUrl: string,
  init: RequestInit = {},
  tokenProvider: typeof getAccessToken = getAccessToken,
): Promise<T> {
  const url = pathOrUrl.startsWith("http") ? pathOrUrl : `${API}${pathOrUrl}`;
  let forceRefresh = false;
  for (let attempt = 1; ; attempt++) {
    const token = await tokenProvider(spotifyUserId, forceRefresh);
    const response = await fetch(url, {
      ...init,
      headers: { Authorization: `Bearer ${token}`, "Content-Type": "application/json", ...init.headers },
    });
    if (response.ok) {
      const text = await response.text();
      return (text ? JSON.parse(text) : null) as T;
    }
    const retryable = response.status === 429 || response.status >= 500 || (response.status === 401 && !forceRefresh);
    if (!retryable || attempt >= MAX_ATTEMPTS) {
      logger.warn("spotify api error", { url, status: response.status });
      if (response.status === 404) throw new AppError(404, "SPOTIFY_NOT_FOUND", "Risorsa Spotify non trovata");
      if (response.status === 401) throw new AppError(401, "SPOTIFY_UNAUTHORIZED", "Sessione Spotify non valida");
      throw new AppError(502, "SPOTIFY_API_ERROR", `Errore API Spotify (${response.status})`);
    }
    if (response.status === 401) {
      forceRefresh = true;
      continue;
    }
    const retryAfter = Number(response.headers.get("Retry-After"));
    await Bun.sleep(retryAfter > 0 ? retryAfter * 1000 : 2 ** attempt * 250);
  }
}

/** Follows Spotify `next` links, collecting items. `pick` extracts the paging object from each page. */
export async function paginate<T>(
  spotifyUserId: string,
  path: string,
  pick: (page: any) => { items: T[]; next: string | null } = (page) => page,
): Promise<T[]> {
  const items: T[] = [];
  let next: string | null = path;
  while (next) {
    const page = pick(await spotifyFetch(spotifyUserId, next));
    items.push(...page.items);
    next = page.next;
  }
  return items;
}
