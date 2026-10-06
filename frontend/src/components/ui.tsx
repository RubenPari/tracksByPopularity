import { AlertCircle, Inbox, Loader2 } from "lucide-react";
import { useEffect, useRef, type ReactNode } from "react";

export function Spinner({ label }: { label?: string }) {
  return (
    <div role="status" className="flex items-center gap-2 text-muted">
      <Loader2 className="size-5 animate-spin text-spotify" aria-hidden />
      <span className={label ? "" : "sr-only"}>{label ?? "Caricamento..."}</span>
    </div>
  );
}

export function Skeleton({ className = "" }: { className?: string }) {
  return <div className={`animate-pulse rounded-lg bg-highlight ${className}`} aria-hidden />;
}

export function PageHeader({ title, subtitle, actions }: { title: string; subtitle?: string; actions?: ReactNode }) {
  return (
    <header className="mb-8 flex flex-wrap items-end justify-between gap-4">
      <div>
        <h1 className="font-display text-3xl tracking-wide text-foreground md:text-4xl">{title}</h1>
        {subtitle && <p className="mt-2 max-w-2xl text-sm text-muted">{subtitle}</p>}
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
      className="m-auto w-[min(92vw,28rem)] rounded-2xl border border-white/10 bg-elevated p-6 text-foreground shadow-2xl shadow-spotify/10"
    >
      <h2 id="confirm-title" className="font-display text-xl tracking-wide">
        {title}
      </h2>
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
    <div className="card flex flex-col items-center gap-4 py-10 text-center" role="alert">
      <span className="grid size-12 place-items-center rounded-full bg-destructive/15 text-destructive">
        <AlertCircle className="size-6" aria-hidden />
      </span>
      <p className="max-w-sm text-sm text-muted">
        Impossibile caricare i dati. Controlla la connessione o riaccedi a Spotify.
      </p>
      <button type="button" className="btn-secondary" onClick={onRetry}>
        Riprova
      </button>
    </div>
  );
}

export function EmptyState({ children, action }: { children: ReactNode; action?: ReactNode }) {
  return (
    <div className="card flex flex-col items-center gap-4 py-10 text-center">
      <span className="grid size-12 place-items-center rounded-full bg-highlight text-muted">
        <Inbox className="size-6" aria-hidden />
      </span>
      <p className="max-w-sm text-sm text-muted">{children}</p>
      {action}
    </div>
  );
}
