import { zodResolver } from "@hookform/resolvers/zod";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { Link2, Link2Off, LogOut } from "lucide-react";
import { useForm } from "react-hook-form";
import { Link, useNavigate } from "react-router";
import { toast } from "sonner";
import { z } from "zod";
import { api, fetchData, send } from "../api";
import { PageHeader } from "../components/ui";
import { useSession } from "../session";

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

  const afterLogout = async (message: string) => {
    toast.success(message);
    queryClient.clear();
    await queryClient.invalidateQueries();
    navigate("/login");
  };

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
      <PageHeader title="Account" />
      <div className="grid gap-6 lg:grid-cols-2">
        <section className="card space-y-4" aria-labelledby="profile-title">
          <h2 id="profile-title" className="text-lg font-bold">Profilo</h2>
          {account ? (
            <dl className="grid grid-cols-[auto_1fr] gap-x-6 gap-y-2 text-sm">
              <dt className="text-muted">Email</dt>
              <dd>{account.email}</dd>
              <dt className="text-muted">Spotify</dt>
              <dd>{account.spotifyLinked ? `Collegato (${account.spotifyUserId})` : "Non collegato"}</dd>
            </dl>
          ) : (
            <p className="text-sm text-muted">
              Sei connesso solo con Spotify{spotifyUserId ? ` (${spotifyUserId})` : ""}.{" "}
              <Link to="/login" className="text-spotify underline">Crea un account locale</Link> per conservare il collegamento.
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
              <button type="button" className="btn-secondary" onClick={() => logoutSpotify.mutate()} disabled={logoutSpotify.isPending}>
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
    { name: "currentPassword", label: "Password attuale", autoComplete: "current-password" },
    { name: "newPassword", label: "Nuova password", autoComplete: "new-password" },
    { name: "confirmPassword", label: "Conferma nuova password", autoComplete: "new-password" },
  ] as const;

  return (
    <section className="card" aria-labelledby="password-title">
      <h2 id="password-title" className="mb-4 text-lg font-bold">Cambia password</h2>
      <form className="space-y-4" noValidate onSubmit={form.handleSubmit((values) => change.mutate(values))}>
        {fields.map(({ name, label, autoComplete }) => (
          <label key={name} className="block text-sm">
            <span className="mb-1 block font-semibold">{label}</span>
            <input className="input" type="password" autoComplete={autoComplete} aria-invalid={!!errors[name]} {...form.register(name)} />
            {errors[name] && <span className="mt-1 block text-xs text-red-400">{errors[name].message}</span>}
          </label>
        ))}
        <button type="submit" className="btn-primary" disabled={change.isPending}>
          {change.isPending ? "Salvataggio..." : "Aggiorna password"}
        </button>
      </form>
    </section>
  );
}
