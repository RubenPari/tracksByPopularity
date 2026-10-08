import { revokeSpotifySessions } from "../plugins/session";
import { deleteToken } from "../spotify/tokens";
import { invalidateUserLibraryCache } from "./library";

/**
 * Full Spotify teardown for logout/unlink: revoke all sessions, drop token cache, invalidate library cache.
 */
export async function clearSpotifyAccess(spotifyUserId: string) {
  await revokeSpotifySessions(spotifyUserId);
  await deleteToken(spotifyUserId);
  await invalidateUserLibraryCache(spotifyUserId);
}
