function required(name: string): string {
  const value = process.env[name];
  if (!value) throw new Error(`Missing required env variable: ${name}`);
  return value;
}

const jwtSecret = required("JWT_SECRET");
if (jwtSecret.length < 32) throw new Error("JWT_SECRET must be at least 32 characters");

const spotifyRedirectUri = required("SPOTIFY_REDIRECT_URI");

export const config = {
  port: Number(process.env.PORT ?? 3000),
  isProduction: process.env.BUN_ENV === "production",
  jwtSecret,
  databaseUrl: required("DATABASE_URL"),
  spotify: {
    clientId: required("SPOTIFY_CLIENT_ID"),
    clientSecret: required("SPOTIFY_CLIENT_SECRET"),
    redirectUri: spotifyRedirectUri,
    linkRedirectUri: `${new URL(spotifyRedirectUri).origin}/api/spotify/callback`,
  },
  redis: {
    host: process.env.REDIS_HOST ?? "localhost",
    port: Number(process.env.REDIS_PORT ?? 6379),
    password: process.env.REDIS_PASSWORD || undefined,
  },
  frontendOrigin: process.env.FRONTEND_ORIGIN ?? "http://localhost:5173",
};
