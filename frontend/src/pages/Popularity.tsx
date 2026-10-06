import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { RefreshCw } from "lucide-react";
import { useState } from "react";
import { toast } from "sonner";
import { api, fetchData, send } from "../api";
import { ConfirmDialog, EmptyState, ErrorState, PageHeader, Skeleton } from "../components/ui";
import { POPULARITY_RANGES, type PopularityRange } from "../lib";

export function Popularity() {
  const [range, setRange] = useState<PopularityRange>("medium");
  const [confirming, setConfirming] = useState(false);
  const queryClient = useQueryClient();

  const preview = useQuery({
    queryKey: ["preview", range],
    queryFn: () => fetchData(api.api.track.popularity({ range }).preview.get()),
  });
  const sync = useMutation({
    mutationFn: () => send(api.api.track.popularity({ range }).post()),
    onSuccess: ({ data }) => {
      toast.success(`"${data!.playlistName}" aggiornata con ${data!.trackCount} brani`);
      setConfirming(false);
      for (const key of ["dashboard", "backups", "playlists"]) queryClient.invalidateQueries({ queryKey: [key] });
    },
  });

  return (
    <>
      <PageHeader
        title="Popolarità"
        subtitle="Scegli una fascia: i brani salvati che ne fanno parte finiranno nella playlist dedicata."
      />

      <div role="radiogroup" aria-label="Fascia di popolarità" className="mb-6 grid grid-cols-2 gap-3 sm:grid-cols-5">
        {POPULARITY_RANGES.map((option, i) => (
          <button
            key={option.key}
            type="button"
            role="radio"
            aria-checked={range === option.key}
            onClick={() => setRange(option.key)}
            className={`card motion-safe:animate-fade-up min-h-[5.5rem] cursor-pointer text-left focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-white ${
              ["stagger-1", "stagger-2", "stagger-3", "stagger-4", "stagger-5"][i] ?? ""
            } ${
              range === option.key
                ? "border-spotify bg-spotify/10 shadow-[var(--shadow-glow)] ring-2 ring-spotify"
                : "hover:border-white/20 hover:bg-highlight"
            }`}
          >
            <p className="font-display text-xl tracking-wide">{option.label}</p>
            <p className="mt-1 text-xs text-muted">{option.description}</p>
          </button>
        ))}
      </div>

      <section className="card border-white/10">
        <div className="mb-4 flex flex-wrap items-center justify-between gap-3">
          <div>
            <h2 className="font-display text-xl tracking-wide">{preview.data?.playlistName ?? "Anteprima"}</h2>
            <p className="text-sm text-muted">
              {preview.data ? `${preview.data.trackCount} brani in questa fascia` : "Calcolo anteprima..."}
            </p>
          </div>
          <button
            type="button"
            className="btn-primary"
            onClick={() => setConfirming(true)}
            disabled={!preview.data || sync.isPending}
          >
            <RefreshCw className={`size-4 ${sync.isPending ? "animate-spin" : ""}`} aria-hidden />
            {sync.isPending ? "Sincronizzazione..." : "Sincronizza playlist"}
          </button>
        </div>

        {preview.isPending ? (
          <div className="space-y-3" aria-busy="true">
            {Array.from({ length: 6 }, (_, i) => (
              <div key={i} className="flex items-center gap-4">
                <Skeleton className="h-4 w-8" />
                <div className="min-w-0 flex-1 space-y-2">
                  <Skeleton className="h-4 w-48 max-w-full" />
                  <Skeleton className="h-3 w-32" />
                </div>
                <Skeleton className="h-6 w-10 rounded-full" />
              </div>
            ))}
          </div>
        ) : preview.isError ? (
          <ErrorState onRetry={() => preview.refetch()} />
        ) : preview.data?.tracks.length ? (
          <ol className="max-h-[28rem] divide-y divide-white/5 overflow-y-auto">
            {preview.data.tracks.map((track, i) => (
              <li key={track.id} className="flex items-center gap-4 py-2.5 text-sm">
                <span className="w-8 text-right text-muted tabular-nums">{i + 1}</span>
                <div className="min-w-0 flex-1">
                  <p className="truncate font-medium">{track.name}</p>
                  <p className="truncate text-xs text-muted">{track.artists.map((a) => a.name).join(", ")}</p>
                </div>
                <span
                  className="rounded-full border border-accent/30 bg-accent/15 px-2.5 py-0.5 text-xs font-semibold text-accent tabular-nums"
                  title="Popolarità"
                >
                  {track.popularity}
                </span>
              </li>
            ))}
          </ol>
        ) : (
          <EmptyState>Nessun brano salvato in questa fascia.</EmptyState>
        )}
      </section>

      <ConfirmDialog
        open={confirming}
        title={`Sincronizzare "${preview.data?.playlistName}"?`}
        confirmLabel="Sincronizza"
        pending={sync.isPending}
        onConfirm={() => sync.mutate()}
        onClose={() => setConfirming(false)}
      >
        <p>La playlist verrà svuotata e riempita con {preview.data?.trackCount ?? 0} brani.</p>
        <p>Prima di modificarla viene salvato uno snapshot, ripristinabile dalla sezione Backup.</p>
      </ConfirmDialog>
    </>
  );
}
