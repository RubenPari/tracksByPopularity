import { useQuery } from "@tanstack/react-query";
import { Archive, BarChart3, ListMusic, Mic2, Music } from "lucide-react";
import { Link } from "react-router";
import { api, fetchData } from "../api";
import { ErrorState, PageHeader, Skeleton, Spinner } from "../components/ui";

export function Dashboard() {
  const dashboard = useQuery({ queryKey: ["dashboard"], queryFn: () => fetchData(api.api.dashboard.get()) });
  if (dashboard.isPending) {
    return (
      <div className="space-y-8" aria-busy="true">
        <div className="flex items-center gap-5">
          <Skeleton className="size-24 rounded-full" />
          <div className="space-y-3">
            <Skeleton className="h-5 w-36" />
            <Skeleton className="h-10 w-56" />
          </div>
        </div>
        <div className="grid gap-4 sm:grid-cols-3">
          <Skeleton className="h-32" />
          <Skeleton className="h-32" />
          <Skeleton className="h-32" />
        </div>
        <Spinner label="Analisi della libreria..." />
      </div>
    );
  }
  if (!dashboard.data) return <ErrorState onRetry={() => dashboard.refetch()} />;
  const { profile, savedTracks, playlists, managedPlaylists } = dashboard.data;

  return (
    <>
      <section className="mb-10 flex flex-wrap items-center gap-5">
        {profile.image ? (
          <img
            src={profile.image}
            alt=""
            className="size-28 rounded-full object-cover shadow-xl shadow-spotify/20 ring-2 ring-spotify/40"
          />
        ) : (
          <span className="font-display grid size-28 place-items-center rounded-full bg-highlight text-4xl ring-2 ring-spotify/40">
            {profile.displayName.slice(0, 1).toUpperCase()}
          </span>
        )}
        <div>
          <span className="inline-flex items-center gap-1.5 rounded-full border border-accent/40 bg-accent/15 px-3 py-1 text-xs font-semibold text-accent">
            <span className="size-2 rounded-full bg-spotify" aria-hidden /> Spotify connesso
          </span>
          <h1 className="font-display mt-3 text-4xl tracking-wide md:text-5xl">Ciao, {profile.displayName}</h1>
        </div>
      </section>

      <div className="mb-10 grid gap-4 sm:grid-cols-3">
        <Stat icon={Music} label="Brani salvati" value={savedTracks} delay="stagger-1" />
        <Stat icon={ListMusic} label="Playlist possedute" value={playlists} delay="stagger-2" />
        <Stat icon={BarChart3} label="Playlist gestite" value={managedPlaylists} delay="stagger-3" />
      </div>

      <PageHeader title="Azioni rapide" subtitle="Parti da qui per organizzare la libreria." />
      <div className="grid gap-4 sm:grid-cols-3">
        <QuickLink
          to="/popularity"
          icon={BarChart3}
          title="Per popolarità"
          text="Crea playlist per fascia 0-100"
          delay="stagger-1"
        />
        <QuickLink
          to="/artists"
          icon={Mic2}
          title="Per artista"
          text="Dividi un artista in 3 playlist"
          delay="stagger-2"
        />
        <QuickLink to="/backups" icon={Archive} title="Backup" text="Ripristina uno snapshot" delay="stagger-3" />
      </div>
    </>
  );
}

function Stat({
  icon: Icon,
  label,
  value,
  delay,
}: {
  icon: typeof Music;
  label: string;
  value: number;
  delay: string;
}) {
  return (
    <div className={`card motion-safe:animate-fade-up ${delay} border-white/10`}>
      <Icon className="mb-4 size-7 text-spotify" aria-hidden />
      <p className="font-display text-4xl tracking-wide">{value.toLocaleString("it-IT")}</p>
      <p className="mt-1 text-sm text-muted">{label}</p>
    </div>
  );
}

function QuickLink({
  to,
  icon: Icon,
  title,
  text,
  delay,
}: {
  to: string;
  icon: typeof Music;
  title: string;
  text: string;
  delay: string;
}) {
  return (
    <Link
      to={to}
      className={`card group motion-safe:animate-fade-up ${delay} focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-white hover:border-spotify/40 hover:bg-highlight hover:shadow-[var(--shadow-glow)]`}
    >
      <Icon className="mb-3 size-7 text-muted transition duration-200 group-hover:text-spotify" aria-hidden />
      <p className="font-display text-lg tracking-wide">{title}</p>
      <p className="mt-1 text-sm text-muted">{text}</p>
    </Link>
  );
}
