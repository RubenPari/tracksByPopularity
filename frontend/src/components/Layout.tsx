import { Archive, AudioLines, BarChart3, LayoutDashboard, Mic2, UserRound } from "lucide-react";
import { Navigate, NavLink, Outlet } from "react-router";
import { useOAuthReturn, useSession } from "../session";
import { Spinner } from "./ui";

const NAV = [
  { to: "/", label: "Dashboard", icon: LayoutDashboard, end: true },
  { to: "/popularity", label: "Popolarità", icon: BarChart3 },
  { to: "/artists", label: "Artisti", icon: Mic2 },
  { to: "/backups", label: "Backup", icon: Archive },
  { to: "/account", label: "Account", icon: UserRound },
];

const appName = import.meta.env.VITE_APP_NAME ?? "TracksByPopularity";

export function Layout() {
  useOAuthReturn();
  return (
    <div className="flex min-h-dvh flex-col md:flex-row">
      <aside className="shrink-0 border-b border-white/10 bg-surface md:sticky md:top-0 md:h-dvh md:w-64 md:border-r md:border-b-0 md:p-4">
        <div className="flex items-center gap-3 px-4 py-4 md:px-2">
          <span className="grid size-10 place-items-center rounded-full bg-spotify text-black shadow-[var(--shadow-glow)]">
            <AudioLines className="size-5" aria-hidden />
          </span>
          <span className="font-display text-lg leading-tight tracking-wide">{appName}</span>
        </div>
        <nav aria-label="Principale" className="flex gap-1 overflow-x-auto px-2 pb-3 md:flex-col md:px-0 md:pb-0">
          {NAV.map(({ to, label, icon: Icon, end }) => (
            <NavLink
              key={to}
              to={to}
              end={end}
              className={({ isActive }) =>
                `flex min-h-11 items-center gap-3 rounded-xl px-3 py-2.5 text-sm font-semibold whitespace-nowrap transition duration-200 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-white ${
                  isActive
                    ? "bg-highlight text-white shadow-[inset_3px_0_0_0_var(--color-spotify)]"
                    : "text-muted hover:bg-highlight/60 hover:text-white"
                }`
              }
            >
              <Icon className="size-5 shrink-0" aria-hidden />
              {label}
            </NavLink>
          ))}
        </nav>
      </aside>
      <main className="flex-1 bg-[radial-gradient(ellipse_at_top,_rgb(34_197_94_/_0.12),_transparent_55%),linear-gradient(to_bottom,_#1a1a1a_0%,_#000_45%)] p-4 md:p-8">
        <div className="motion-safe:animate-fade-up mx-auto max-w-6xl">
          <Outlet />
        </div>
      </main>
    </div>
  );
}

/** Pages that call Spotify: need a direct Spotify session or a local account linked to Spotify. */
export function RequireSpotify() {
  const { loading, spotifyAuthenticated, account } = useSession();
  if (loading) return <Spinner />;
  // Local account without Spotify: send to /account to link it.
  if (!spotifyAuthenticated) return <Navigate to={account ? "/account" : "/login"} replace />;
  return <Outlet />;
}

/** Account page: any session (local account or Spotify) is enough. */
export function RequireAnySession() {
  const { loading, spotifyAuthenticated, account } = useSession();
  if (loading) return <Spinner />;
  if (!spotifyAuthenticated && !account) return <Navigate to="/login" replace />;
  return <Outlet />;
}
