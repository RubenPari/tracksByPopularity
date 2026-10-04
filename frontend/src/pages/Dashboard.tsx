import { useQuery } from "@tanstack/react-query";
import { Archive, BarChart3, ListMusic, Mic2, Music } from "lucide-react";
import { Link } from "react-router";
import { api, fetchData } from "../api";
import { ErrorState, PageHeader, Spinner } from "../components/ui";

export function Dashboard() {
  const dashboard = useQuery({ queryKey: ["dashboard"], queryFn: () => fetchData(api.api.dashboard.get()) });
  if (dashboard.isPending) return <Spinner label="Analisi della libreria..." />;
  if (!dashboard.data) return <ErrorState onRetry={() => dashboard.refetch()} />;
  const { profile, savedTracks, playlists, managedPlaylists } = dashboard.data;

  return (
    <>
      <section className="mb-8 flex flex-wrap items-center gap-5">
        {profile.image ? (
          <img src={profile.image} alt="" className="size-24 rounded-full object-cover shadow-xl" />
        ) : (
          <span className="grid size-24 place-items-center rounded-full bg-highlight text-3xl font-bold">
            {profile.displayName.slice(0, 1).toUpperCase()}
          </span>
        )}
        <div>
          <span className="inline-flex items-center gap-1.5 rounded-full bg-spotify/15 px-3 py-1 text-xs font-semibold text-spotify">
            <span className="size-2 rounded-full bg-spotify" aria-hidden /> Spotify connesso
          </span>
          <h1 className="mt-2 text-4xl font-bold tracking-tight">Ciao, {profile.displayName}</h1>
        </div>
      </section>

      <div className="grid gap-4 sm:grid-cols-3">
        <Stat icon={Music} label="Brani salvati" value={savedTracks} />
        <Stat icon={ListMusic} label="Playlist possedute" value={playlists} />
        <Stat icon={BarChart3} label="Playlist gestite" value={managedPlaylists} />
      </div>

      <PageHeader title="Azioni rapide" />
      <div className="grid gap-4 sm:grid-cols-3">
        <QuickLink to="/popularity" icon={BarChart3} title="Per popolarità" text="Crea playlist per fascia 0-100" />
        <QuickLink to="/artists" icon={Mic2} title="Per artista" text="Dividi un artista in 3 playlist" />
        <QuickLink to="/backups" icon={Archive} title="Backup" text="Ripristina uno snapshot" />
      </div>
    </>
  );
}

function Stat({ icon: Icon, label, value }: { icon: typeof Music; label: string; value: number }) {
  return (
    <div className="card mb-8">
      <Icon className="mb-3 size-6 text-spotify" aria-hidden />
      <p className="text-3xl font-bold">{value.toLocaleString("it-IT")}</p>
      <p className="text-sm text-muted">{label}</p>
    </div>
  );
}

function QuickLink({ to, icon: Icon, title, text }: { to: string; icon: typeof Music; title: string; text: string }) {
  return (
    <Link to={to} className="card group transition hover:bg-highlight">
      <Icon className="mb-3 size-6 text-muted group-hover:text-spotify" aria-hidden />
      <p className="font-semibold">{title}</p>
      <p className="text-sm text-muted">{text}</p>
    </Link>
  );
}
