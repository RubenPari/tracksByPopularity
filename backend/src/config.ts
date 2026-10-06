/** Fails fast when a required env var is missing. */
function required(name: string): string {
  const value = process.env[name];
  if (!value) throw new Error(`Missing required env variable: ${name}`);
  return value;
}

const jwtSecret = required("JWT_SECRET");
// Also used to derive the AES-256-GCM key for Spotify tokens at rest.
if (jwtSecret.length < 32) throw new Error("JWT_SECRET must be at least 32 characters");

const spotifyRedirectUri = required("SPOTIFY_REDIRECT_URI");

/** Validated runtime configuration from environment variables. */
export const config = {
  port: Number(process.env.PORT ?? 3000),
  isProduction: process.env.BUN_ENV === "production",
  jwtSecret,
  databaseUrl: required("DATABASE_URL"),
  spotify: {
    clientId: required("SPOTIFY_CLIENT_ID"),
    clientSecret: required("SPOTIFY_CLIENT_SECRET"),
    /** Redirect for Spotify-only login (`/auth/callback`). */
    redirectUri: spotifyRedirectUri,
    /** Redirect for linking Spotify to a local account (`/api/spotify/callback`). */
    linkRedirectUri: `${new URL(spotifyRedirectUri).origin}/api/spotify/callback`,
  },
  redis: {
    host: process.env.REDIS_HOST ?? "localhost",
    port: Number(process.env.REDIS_PORT ?? 6379),
    password: process.env.REDIS_PASSWORD || undefined,
  },
  /** Allowed CORS origin and OAuth post-redirect target. */
  frontendOrigin: process.env.FRONTEND_ORIGIN ?? "http://localhost:5173",
};
