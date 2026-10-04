import { Loader2 } from "lucide-react";
import { useEffect, useRef, type ReactNode } from "react";

export function Spinner({ label }: { label?: string }) {
  return (
    <div role="status" className="flex items-center gap-2 text-muted">
      <Loader2 className="size-5 animate-spin" aria-hidden />
      <span className={label ? "" : "sr-only"}>{label ?? "Caricamento..."}</span>
    </div>
  );
}

export function PageHeader({ title, subtitle, actions }: { title: string; subtitle?: string; actions?: ReactNode }) {
  return (
    <header className="mb-6 flex flex-wrap items-end justify-between gap-4">
      <div>
        <h1 className="text-3xl font-bold tracking-tight">{title}</h1>
        {subtitle && <p className="mt-1 text-sm text-muted">{subtitle}</p>}
      </div>
      {actions}
    </header>
  );
}

type ConfirmDialogProps = {
  open: boolean;
  title: string;
  children: ReactNode;
  confirmLabel: string;
  danger?: boolean;
  pending?: boolean;
  onConfirm: () => void;
  onClose: () => void;
};

/** Modal confirmation built on the native <dialog> (focus trap + Esc for free). */
export function ConfirmDialog({ open, title, children, confirmLabel, danger, pending, onConfirm, onClose }: ConfirmDialogProps) {
  const ref = useRef<HTMLDialogElement>(null);
  useEffect(() => {
    const dialog = ref.current;
    if (open && !dialog?.open) dialog?.showModal();
    if (!open && dialog?.open) dialog.close();
  }, [open]);
  return (
    <dialog
      ref={ref}
      onClose={onClose}
      aria-labelledby="confirm-title"
      className="m-auto w-[min(92vw,28rem)] rounded-xl bg-elevated p-6 text-white shadow-2xl"
    >
      <h2 id="confirm-title" className="text-lg font-bold">{title}</h2>
      <div className="mt-3 space-y-2 text-sm text-muted">{children}</div>
      <div className="mt-6 flex justify-end gap-3">
        <button type="button" className="btn-secondary" onClick={onClose} disabled={pending}>
          Annulla
        </button>
        <button type="button" className={danger ? "btn-danger" : "btn-primary"} onClick={onConfirm} disabled={pending}>
          {pending && <Loader2 className="size-4 animate-spin" aria-hidden />}
          {confirmLabel}
        </button>
      </div>
    </dialog>
  );
}

export function ErrorState({ onRetry }: { onRetry: () => void }) {
  return (
    <div className="card flex flex-col items-center gap-3 text-center text-sm text-muted">
      <p>Impossibile caricare i dati. Controlla la connessione o riaccedi a Spotify.</p>
      <button type="button" className="btn-secondary" onClick={onRetry}>
        Riprova
      </button>
    </div>
  );
}

export function EmptyState({ children }: { children: ReactNode }) {
  return <p className="card text-center text-sm text-muted">{children}</p>;
}
