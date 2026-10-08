import { zodResolver } from "@hookform/resolvers/zod";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { Link2, Link2Off, LogOut } from "lucide-react";
import { useForm } from "react-hook-form";
import { Link, useNavigate } from "react-router";
import { toast } from "sonner";
import { z } from "zod";
import { api, fetchData, send } from "../api";
import { PageHeader } from "../components/ui";
import { resetSession, useSession } from "../session";

const passwordSchema = z
  .object({
    currentPassword: z.string().min(1, "Inserisci la password attuale"),
    newPassword: z.string().min(8, "Almeno 8 caratteri").max(128),
    confirmPassword: z.string(),
  })
  .refine((v) => v.newPassword === v.confirmPassword, { path: ["confirmPassword"], message: "Le password non coincidono" });
type PasswordForm = z.infer<typeof passwordSchema>;

export function Account() {
  const { account, spotifyAuthenticated, spotifyUserId } = useSession();
  const queryClient = useQueryClient();
  const navigate = useNavigate();

  const afterLogout = (message: string) => resetSession(queryClient, navigate, message);

  const link = useMutation({
    mutationFn: async () => {
      // Already in a Spotify session: link it directly; otherwise go through OAuth.
      if (spotifyAuthenticated) return send(api.api.account["link-spotify"].post());
      const { url } = await fetchData(api.api.spotify["link-url"].get());
      window.location.assign(url);
      return null;
    },
    onSuccess: (response) => {
      if (!response) return;
      toast.success(response.message);
      queryClient.invalidateQueries();
    },
  });
  const unlink = useMutation({
    mutationFn: () => send(api.api.spotify.unlink.post()),
    onSuccess: (response) => afterLogout(response.message),
  });
  const logoutLocal = useMutation({
    mutationFn: () => send(api.api.account.logout.post()),
    onSuccess: (response) => afterLogout(response.message),
  });
  const logoutSpotify = useMutation({
    mutationFn: () => send(api.auth.logout.post()),
    onSuccess: (response) => afterLogout(response.message),
  });

  return (
    <>
      <PageHeader title="Account" subtitle="Gestisci profilo, collegamento Spotify e password." />
      <div className="grid gap-6 lg:grid-cols-2">
        <section className="card space-y-4 border-white/10" aria-labelledby="profile-title">
          <h2 id="profile-title" className="font-display text-xl tracking-wide">
            Profilo
          </h2>
          {account ? (
            <dl className="grid grid-cols-[auto_1fr] gap-x-6 gap-y-3 text-sm">
              <dt className="text-muted">Email</dt>
              <dd className="font-medium">{account.email}</dd>
              <dt className="text-muted">Spotify</dt>
              <dd>
                {account.spotifyLinked ? (
                  <span className="inline-flex items-center gap-1.5 rounded-full border border-spotify/40 bg-spotify/15 px-2.5 py-0.5 text-xs font-semibold text-spotify">
                    Collegato ({account.spotifyUserId})
                  </span>
                ) : (
                  <span className="inline-flex items-center rounded-full border border-accent/40 bg-accent/15 px-2.5 py-0.5 text-xs font-semibold text-accent">
                    Non collegato
                  </span>
                )}
              </dd>
            </dl>
          ) : (
            <p className="text-sm text-muted">
              Sei connesso solo con Spotify{spotifyUserId ? ` (${spotifyUserId})` : ""}.{" "}
              <Link to="/login" className="font-semibold text-spotify underline-offset-2 hover:underline">
                Crea un account locale
              </Link>{" "}
              per conservare il collegamento.
            </p>
          )}

          <div className="flex flex-wrap gap-3 pt-2">
            {account && !account.spotifyLinked && (
              <button type="button" className="btn-primary" onClick={() => link.mutate()} disabled={link.isPending}>
                <Link2 className="size-4" aria-hidden /> Collega Spotify
              </button>
            )}
            {account?.spotifyLinked && (
              <button type="button" className="btn-secondary" onClick={() => unlink.mutate()} disabled={unlink.isPending}>
                <Link2Off className="size-4" aria-hidden /> Scollega Spotify
              </button>
            )}
            {account && (
              <button type="button" className="btn-secondary" onClick={() => logoutLocal.mutate()} disabled={logoutLocal.isPending}>
                <LogOut className="size-4" aria-hidden /> Esci
              </button>
            )}
            {!account && spotifyAuthenticated && (
              <button
                type="button"
                className="btn-secondary"
                onClick={() => logoutSpotify.mutate()}
                disabled={logoutSpotify.isPending}
              >
                <LogOut className="size-4" aria-hidden /> Esci da Spotify
              </button>
            )}
          </div>
        </section>

        {account && <ChangePassword />}
      </div>
    </>
  );
}

function ChangePassword() {
  const form = useForm<PasswordForm>({ resolver: zodResolver(passwordSchema) });
  const change = useMutation({
    mutationFn: ({ currentPassword, newPassword }: PasswordForm) =>
      send(api.api.account["change-password"].post({ currentPassword, newPassword })),
    onSuccess: (response) => {
      toast.success(response.message);
      form.reset();
    },
  });
  const { errors } = form.formState;
  const fields = [
    { name: "currentPassword", label: "Password attuale", autoComplete: "current-password", errorId: "current-password-error" },
    { name: "newPassword", label: "Nuova password", autoComplete: "new-password", errorId: "new-password-error" },
    { name: "confirmPassword", label: "Conferma nuova password", autoComplete: "new-password", errorId: "confirm-password-error" },
  ] as const;

  return (
    <section className="card border-white/10" aria-labelledby="password-title">
      <h2 id="password-title" className="font-display mb-4 text-xl tracking-wide">
        Cambia password
      </h2>
      <form className="space-y-4" noValidate onSubmit={form.handleSubmit((values) => change.mutate(values))}>
        {fields.map(({ name, label, autoComplete, errorId }) => (
          <label key={name} className="block text-sm">
            <span className="mb-1.5 block font-semibold">{label}</span>
            <input
              className="input"
              type="password"
              autoComplete={autoComplete}
              aria-invalid={!!errors[name]}
              aria-describedby={errors[name] ? errorId : undefined}
              {...form.register(name)}
            />
            {errors[name] && (
              <span id={errorId} role="alert" className="mt-1 block text-xs text-red-400">
                {errors[name].message}
              </span>
            )}
          </label>
        ))}
        <button type="submit" className="btn-primary" disabled={change.isPending}>
          {change.isPending ? "Salvataggio..." : "Aggiorna password"}
        </button>
      </form>
    </section>
  );
}
