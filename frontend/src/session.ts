import { useQuery, useQueryClient } from "@tanstack/react-query";
import { useEffect } from "react";
import { useSearchParams } from "react-router";
import { toast } from "sonner";
import { api, fetchData } from "./api";

/** Spotify session (direct or via linked account) + optional local account. */
export function useSession() {
  const spotify = useQuery({
    queryKey: ["is-auth"],
    queryFn: () => fetchData(api.auth["is-auth"].get()),
  });
  const account = useQuery({
    queryKey: ["me"],
    queryFn: async () => {
      const { data, error } = await api.api.account.me.get();
      if (error) return null; // not logged in with a local account
      return data.data;
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
      queryClient.invalidateQueries();
    }
  }, [params, setParams, queryClient]);
}
