import { Archive, BarChart3, LayoutDashboard, Mic2, UserRound } from "lucide-react";
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
      <aside className="shrink-0 bg-surface md:sticky md:top-0 md:h-dvh md:w-60 md:p-4">
        <div className="flex items-center gap-2 px-4 py-4 text-lg font-bold md:px-2">
          <span className="grid size-8 place-items-center rounded-full bg-spotify text-black">♪</span>
          {appName}
        </div>
        <nav aria-label="Principale" className="flex gap-1 overflow-x-auto px-2 pb-2 md:flex-col md:px-0">
          {NAV.map(({ to, label, icon: Icon, end }) => (
            <NavLink
              key={to}
              to={to}
              end={end}
              className={({ isActive }) =>
                `flex items-center gap-3 rounded-md px-3 py-2 text-sm font-semibold whitespace-nowrap transition ${
                  isActive ? "bg-highlight text-white" : "text-muted hover:text-white"
                }`
              }
            >
              <Icon className="size-5" aria-hidden />
              {label}
            </NavLink>
          ))}
        </nav>
      </aside>
      <main className="flex-1 bg-gradient-to-b from-highlight/60 to-black to-40% p-4 md:p-8">
        <div className="mx-auto max-w-6xl">
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
