import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { RotateCcw, Trash2 } from "lucide-react";
import { useState } from "react";
import { toast } from "sonner";
import { api, fetchData, send } from "../api";
import { ConfirmDialog, EmptyState, ErrorState, PageHeader, Skeleton } from "../components/ui";
import { formatDate, OPERATION_LABELS } from "../lib";

type Pending = { action: "restore" | "delete"; id: string; playlistName: string; trackCount: number };

export function Backups() {
  const [pending, setPending] = useState<Pending | null>(null);
  const queryClient = useQueryClient();
  const backups = useQuery({ queryKey: ["backups"], queryFn: () => fetchData(api.api.backup.list.get()) });

  const action = useMutation({
    mutationFn: ({ action, id }: Pending): Promise<{ message: string }> =>
      action === "restore" ? send(api.api.backup.restore({ id }).post()) : send(api.api.backup({ id }).delete()),
    onSuccess: (response) => {
      toast.success(response.message);
      setPending(null);
      for (const key of ["dashboard", "backups", "playlists"]) queryClient.invalidateQueries({ queryKey: [key] });
    },
  });

  return (
    <>
      <PageHeader title="Backup" subtitle="Snapshot creati automaticamente prima di ogni modifica. Conservati per 30 giorni." />

      {backups.isPending ? (
        <div className="card space-y-3 p-5" aria-busy="true">
          {Array.from({ length: 5 }, (_, i) => (
            <Skeleton key={i} className="h-12 w-full" />
          ))}
        </div>
      ) : backups.isError ? (
        <ErrorState onRetry={() => backups.refetch()} />
      ) : !backups.data?.length ? (
        <EmptyState>Nessuno snapshot: verranno creati alla prima sincronizzazione.</EmptyState>
      ) : (
        <div className="card overflow-x-auto border-white/10 p-0">
          <table className="w-full text-left text-sm">
            <thead className="border-b border-white/10 text-xs text-muted uppercase">
              <tr>
                <th scope="col" className="px-5 py-3 font-medium">
                  Data
                </th>
                <th scope="col" className="px-5 py-3 font-medium">
                  Playlist
                </th>
                <th scope="col" className="px-5 py-3 font-medium">
                  Operazione
                </th>
                <th scope="col" className="px-5 py-3 text-right font-medium">
                  Brani
                </th>
                <th scope="col" className="px-5 py-3">
                  <span className="sr-only">Azioni</span>
                </th>
              </tr>
            </thead>
            <tbody className="divide-y divide-white/5">
              {backups.data.map((snapshot) => (
                <tr key={snapshot.id} className="transition-colors duration-150 hover:bg-highlight/50">
                  <td className="px-5 py-3 whitespace-nowrap text-muted">{formatDate(snapshot.createdAt)}</td>
                  <td className="px-5 py-3 font-medium">{snapshot.playlistName}</td>
                  <td className="px-5 py-3">
                    <span className="rounded-full border border-white/10 bg-highlight px-2.5 py-0.5 text-xs text-muted">
                      {OPERATION_LABELS[snapshot.operationType] ?? snapshot.operationType}
                    </span>
                  </td>
                  <td className="px-5 py-3 text-right tabular-nums">{snapshot.trackCount}</td>
                  <td className="px-5 py-3">
                    <div className="flex justify-end gap-2">
                      <button
                        type="button"
                        className="btn-secondary min-h-10 px-3 py-1.5 text-xs"
                        onClick={() => setPending({ action: "restore", ...snapshot })}
                      >
                        <RotateCcw className="size-4" aria-hidden /> Ripristina
                      </button>
                      <button
                        type="button"
                        className="btn min-h-10 min-w-10 rounded-full p-2 text-muted hover:bg-destructive/20 hover:text-red-400"
                        aria-label={`Elimina snapshot di ${snapshot.playlistName}`}
                        onClick={() => setPending({ action: "delete", ...snapshot })}
                      >
                        <Trash2 className="size-4" aria-hidden />
                      </button>
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}

      <ConfirmDialog
        open={!!pending}
        title={pending?.action === "restore" ? `Ripristinare "${pending.playlistName}"?` : "Eliminare lo snapshot?"}
        confirmLabel={pending?.action === "restore" ? "Ripristina" : "Elimina"}
        danger={pending?.action === "delete"}
        pending={action.isPending}
        onConfirm={() => pending && action.mutate(pending)}
        onClose={() => setPending(null)}
      >
        {pending?.action === "restore" ? (
          <>
            <p>La playlist su Spotify tornerà esattamente allo stato salvato ({pending.trackCount} brani, stesso ordine).</p>
            <p>Lo stato attuale viene a sua volta salvato in un nuovo snapshot, quindi puoi annullare.</p>
          </>
        ) : (
          <p>Lo snapshot di "{pending?.playlistName}" verrà eliminato definitivamente. La playlist su Spotify non cambia.</p>
        )}
      </ConfirmDialog>
    </>
  );
}
