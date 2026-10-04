import { zodResolver } from "@hookform/resolvers/zod";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { useState } from "react";
import { useForm } from "react-hook-form";
import { Navigate, useNavigate } from "react-router";
import { toast } from "sonner";
import { z } from "zod";
import { api, fetchData, send } from "../api";
import { Spinner } from "../components/ui";
import { useOAuthReturn, useSession } from "../session";

const credentialsSchema = z.object({
  email: z.email("Email non valida"),
  password: z.string().min(8, "Almeno 8 caratteri").max(128),
});
type Credentials = z.infer<typeof credentialsSchema>;

export async function startSpotifyLogin() {
  const { url } = await fetchData(api.auth.login.get());
  window.location.assign(url);
}

export function Login() {
  useOAuthReturn();
  const { loading, spotifyAuthenticated } = useSession();
  const [mode, setMode] = useState<"login" | "register">("login");
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const form = useForm<Credentials>({ resolver: zodResolver(credentialsSchema) });

  const spotifyLogin = useMutation({ mutationFn: startSpotifyLogin });
  const submit = useMutation({
    mutationFn: async (values: Credentials) => {
      if (mode === "register") await send(api.api.account.register.post(values));
      return send(api.api.account.login.post(values));
    },
    onSuccess: async (response) => {
      toast.success(mode === "register" ? "Account creato, benvenuto!" : response.message);
      await queryClient.invalidateQueries();
      const session = await fetchData(api.auth["is-auth"].get());
      navigate(session.authenticated ? "/" : "/account");
    },
  });

  if (loading) return <div className="grid min-h-dvh place-items-center"><Spinner /></div>;
  if (spotifyAuthenticated) return <Navigate to="/" replace />;

  const { errors } = form.formState;
  return (
    <main className="grid min-h-dvh place-items-center bg-gradient-to-b from-highlight to-black p-4">
      <div className="w-full max-w-md rounded-2xl bg-surface p-8 shadow-2xl">
        <div className="mb-8 text-center">
          <span className="mx-auto mb-3 grid size-12 place-items-center rounded-full bg-spotify text-2xl text-black">♪</span>
          <h1 className="text-2xl font-bold">{import.meta.env.VITE_APP_NAME ?? "TracksByPopularity"}</h1>
          <p className="mt-1 text-sm text-muted">Organizza la tua libreria Spotify per popolarità e artista</p>
        </div>

        <button
          type="button"
          className="btn-primary w-full py-3"
          onClick={() => spotifyLogin.mutate()}
          disabled={spotifyLogin.isPending}
        >
          Accedi con Spotify
        </button>

        <div className="my-6 flex items-center gap-3 text-xs text-muted">
          <span className="h-px flex-1 bg-white/10" /> oppure con account locale <span className="h-px flex-1 bg-white/10" />
        </div>

        <div role="tablist" className="mb-4 grid grid-cols-2 rounded-full bg-highlight p-1 text-sm font-semibold">
          {(["login", "register"] as const).map((tab) => (
            <button
              key={tab}
              role="tab"
              type="button"
              aria-selected={mode === tab}
              onClick={() => setMode(tab)}
              className={`rounded-full py-1.5 transition ${mode === tab ? "bg-white text-black" : "text-muted"}`}
            >
              {tab === "login" ? "Accedi" : "Registrati"}
            </button>
          ))}
        </div>

        <form className="space-y-4" noValidate onSubmit={form.handleSubmit((values) => submit.mutate(values))}>
          <label className="block text-sm">
            <span className="mb-1 block font-semibold">Email</span>
            <input className="input" type="email" autoComplete="email" aria-invalid={!!errors.email} {...form.register("email")} />
            {errors.email && <span className="mt-1 block text-xs text-red-400">{errors.email.message}</span>}
          </label>
          <label className="block text-sm">
            <span className="mb-1 block font-semibold">Password</span>
            <input
              className="input"
              type="password"
              autoComplete={mode === "login" ? "current-password" : "new-password"}
              aria-invalid={!!errors.password}
              {...form.register("password")}
            />
            {errors.password && <span className="mt-1 block text-xs text-red-400">{errors.password.message}</span>}
          </label>
          <button type="submit" className="btn-secondary w-full py-3" disabled={submit.isPending}>
            {submit.isPending ? "Attendere..." : mode === "login" ? "Accedi" : "Crea account"}
          </button>
        </form>
      </div>
    </main>
  );
}
