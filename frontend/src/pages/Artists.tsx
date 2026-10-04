import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { Search, Split } from "lucide-react";
import { useMemo, useState } from "react";
import { toast } from "sonner";
import { api, fetchData, send } from "../api";
import { ConfirmDialog, EmptyState, ErrorState, PageHeader, Spinner } from "../components/ui";
import { filterArtists, useDebounced, type ArtistSort } from "../lib";

export function Artists() {
  const [query, setQuery] = useState("");
  const [sort, setSort] = useState<ArtistSort>("tracks");
  const [selected, setSelected] = useState<{ id: string; name: string } | null>(null);
  const debouncedQuery = useDebounced(query);
  const queryClient = useQueryClient();

  const artists = useQuery({ queryKey: ["artists"], queryFn: () => fetchData(api.api.track.artists.get()) });
  const visible = useMemo(() => filterArtists(artists.data ?? [], debouncedQuery, sort), [artists.data, debouncedQuery, sort]);

  const split = useMutation({
    mutationFn: (artistId: string) => send(api.api.track.artist.post({ artistId })),
    onSuccess: ({ data }) => {
      const summary = data!.playlists.map((p) => `${p.band}: ${p.trackCount}`).join(" · ");
      toast.success(`${data!.artistName} ripartito (${summary})`);
      setSelected(null);
      for (const key of ["dashboard", "backups", "playlists"]) queryClient.invalidateQueries({ queryKey: [key] });
    },
  });

  return (
    <>
      <PageHeader title="Artisti" subtitle="Artisti che segui e presenti nei tuoi brani salvati." />

      <div className="mb-6 flex flex-wrap gap-3">
        <label className="relative min-w-60 flex-1">
          <span className="sr-only">Cerca artista</span>
          <Search className="pointer-events-none absolute top-1/2 left-3 size-4 -translate-y-1/2 text-muted" aria-hidden />
          <input className="input pl-9" type="search" placeholder="Cerca artista..." value={query} onChange={(e) => setQuery(e.target.value)} />
        </label>
        <label className="flex items-center gap-2 text-sm">
          <span className="text-muted">Ordina per</span>
          <select className="input w-auto" value={sort} onChange={(e) => setSort(e.target.value as ArtistSort)}>
            <option value="tracks">Brani salvati</option>
            <option value="popularity">Popolarità</option>
          </select>
        </label>
      </div>

      {artists.isPending ? (
        <Spinner label="Caricamento artisti..." />
      ) : artists.isError ? (
        <ErrorState onRetry={() => artists.refetch()} />
      ) : visible.length === 0 ? (
        <EmptyState>{artists.data?.length ? "Nessun artista corrisponde alla ricerca." : "Nessun artista seguito nella tua libreria."}</EmptyState>
      ) : (
        <ul className="grid grid-cols-2 gap-4 sm:grid-cols-3 lg:grid-cols-5">
          {visible.map((artist) => (
            <li key={artist.id} className="card group flex flex-col p-4 transition hover:bg-highlight">
              {artist.image ? (
                <img src={artist.image} alt="" loading="lazy" className="mb-3 aspect-square w-full rounded-full object-cover shadow-lg" />
              ) : (
                <span className="mb-3 grid aspect-square w-full place-items-center rounded-full bg-highlight text-3xl font-bold">
                  {artist.name.slice(0, 1)}
                </span>
              )}
              <p className="truncate font-semibold" title={artist.name}>{artist.name}</p>
              <p className="mb-3 text-xs text-muted">
                {artist.trackCount} brani · popolarità {artist.popularity}
              </p>
              <button type="button" className="btn-secondary mt-auto px-3 text-xs" onClick={() => setSelected(artist)}>
                <Split className="size-4" aria-hidden /> Ripartisci
              </button>
            </li>
          ))}
        </ul>
      )}

      <ConfirmDialog
        open={!!selected}
        title={`Ripartire ${selected?.name}?`}
        confirmLabel="Ripartisci in 3 playlist"
        pending={split.isPending}
        onConfirm={() => selected && split.mutate(selected.id)}
        onClose={() => setSelected(null)}
      >
        <p>
          I brani saranno divisi in <strong className="text-white">{selected?.name} less</strong> (0-33),{" "}
          <strong className="text-white">medium</strong> (34-66) e <strong className="text-white">more</strong> (67-100).
        </p>
        <p>Prima della modifica viene creato uno snapshot di ciascuna delle tre playlist.</p>
      </ConfirmDialog>
    </>
  );
}
