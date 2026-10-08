import { useQuery, useQueryClient } from "@tanstack/react-query";
import { useEffect } from "react";
import { useSearchParams } from "react-router";
import { toast } from "sonner";
import { ApiError, api, fetchData } from "./api";

/** Spotify session (direct or via linked account) + optional local account. */
export function useSession() {
  const spotify = useQuery({
    queryKey: ["is-auth"],
    queryFn: () => fetchData(api.auth["is-auth"].get()),
  });
  const account = useQuery({
    queryKey: ["me"],
    queryFn: async () => {
      try {
        return await fetchData(api.api.account.me.get());
      } catch (error) {
        // 401/403 = no local session; other failures must surface so guards do not mis-route.
        if (error instanceof ApiError && (error.status === 401 || error.status === 403)) return null;
        throw error;
      }
    },
    retry: (failureCount, error) => {
      if (error instanceof ApiError && (error.status === 401 || error.status === 403)) return false;
      return failureCount < 2;
    },
  });
  return {
    loading: spotify.isPending || account.isPending,
    spotifyAuthenticated: !!spotify.data?.authenticated,
    spotifyUserId: spotify.data?.spotifyUserId ?? null,
    account: account.data ?? null,
  };
}

const OAUTH_MESSAGES: Record<string, [ok: boolean, message: string]> = {
  "auth:success": [true, "Accesso con Spotify effettuato"],
  "auth:denied": [false, "Accesso a Spotify annullato"],
  "link:success": [true, "Account Spotify collegato"],
  "link:denied": [false, "Collegamento Spotify annullato"],
};

/** Shows the outcome of an OAuth redirect (`?auth=` / `?link=`) and cleans the URL. */
export function useOAuthReturn() {
  const [params, setParams] = useSearchParams();
  const queryClient = useQueryClient();
  useEffect(() => {
    for (const kind of ["auth", "link"]) {
      const outcome = params.get(kind);
      if (!outcome) continue;
      const [ok, message] = OAUTH_MESSAGES[`${kind}:${outcome}`] ?? [false, "Esito OAuth sconosciuto"];
      (ok ? toast.success : toast.error)(message);
      params.delete(kind);
      setParams(params, { replace: true });
      void queryClient.invalidateQueries();
    }
  }, [params, setParams, queryClient]);
}

/** Clears client cache after logout and navigates to login. */
export function resetSession(queryClient: ReturnType<typeof useQueryClient>, navigate: (path: string) => void, message: string) {
  toast.success(message);
  queryClient.clear();
  navigate("/login");
}
