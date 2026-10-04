import { MutationCache, QueryCache, QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { StrictMode } from "react";
import { createRoot } from "react-dom/client";
import { createBrowserRouter, RouterProvider } from "react-router";
import { Toaster, toast } from "sonner";
import { ApiError } from "./api";
import { Layout, RequireAnySession, RequireSpotify } from "./components/Layout";
import "./index.css";
import { Account } from "./pages/Account";
import { Artists } from "./pages/Artists";
import { Backups } from "./pages/Backups";
import { Dashboard } from "./pages/Dashboard";
import { Login } from "./pages/Login";
import { Popularity } from "./pages/Popularity";

const showError = (error: Error) => toast.error(error.message);

const queryClient = new QueryClient({
  queryCache: new QueryCache({ onError: showError }),
  mutationCache: new MutationCache({ onError: showError }),
  defaultOptions: {
    queries: {
      staleTime: 60_000,
      refetchOnWindowFocus: false,
      // Auth errors are final; retry only transient failures.
      retry: (count, error) => !(error instanceof ApiError && error.status < 500) && count < 2,
    },
  },
});

const router = createBrowserRouter([
  { path: "/login", element: <Login /> },
  {
    element: <Layout />,
    children: [
      {
        element: <RequireSpotify />,
        children: [
          { index: true, element: <Dashboard /> },
          { path: "popularity", element: <Popularity /> },
          { path: "artists", element: <Artists /> },
          { path: "backups", element: <Backups /> },
        ],
      },
      { element: <RequireAnySession />, children: [{ path: "account", element: <Account /> }] },
    ],
  },
]);

createRoot(document.getElementById("root")!).render(
  <StrictMode>
    <QueryClientProvider client={queryClient}>
      <RouterProvider router={router} />
      <Toaster theme="dark" position="bottom-right" richColors />
    </QueryClientProvider>
  </StrictMode>,
);
