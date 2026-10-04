import { jwt } from "@elysiajs/jwt";
import { eq } from "drizzle-orm";
import { Elysia } from "elysia";
import { config } from "../config";
import { db } from "../db/client";
import { spotifyLinks } from "../db/schema";
import { unsign } from "../lib/crypto";
import { AppError } from "../lib/response";

export const cookieOptions = (sameSite: "strict" | "lax" = "strict") => ({
  httpOnly: true,
  sameSite,
  secure: config.isProduction,
  path: "/",
});

export const session = new Elysia({ name: "session" })
  .use(jwt({ name: "jwt", secret: config.jwtSecret, exp: "7d" }))
  .derive({ as: "global" }, ({ jwt, cookie, headers }) => {
    /** Local account id from `Authorization: Bearer` or the `access_token` cookie. */
    const resolveUserId = async (): Promise<string | null> => {
      const bearer = headers.authorization?.match(/^Bearer (.+)$/)?.[1];
      const token = bearer ?? (cookie.access_token?.value as string | undefined);
      if (!token) return null;
      const payload = await jwt.verify(token);
      return payload && typeof payload.sub === "string" ? payload.sub : null;
    };
    /** Spotify session: signed header > signed cookie > Spotify account linked to the JWT user. */
    const resolveSpotifyUserId = async (): Promise<string | null> => {
      const direct = unsign(headers["x-spotify-user-id"]) ?? unsign(cookie.spotify_user_id?.value as string | undefined);
      if (direct) return direct;
      const userId = await resolveUserId();
      if (!userId) return null;
      const [link] = await db.select().from(spotifyLinks).where(eq(spotifyLinks.userId, userId));
      return link?.spotifyUserId ?? null;
    };
    return { resolveUserId, resolveSpotifyUserId };
  })
  .macro({
    requireUser: {
      async resolve({ resolveUserId }) {
        const userId = await resolveUserId();
        if (!userId) throw new AppError(401, "UNAUTHORIZED", "Autenticazione richiesta");
        return { userId };
      },
    },
    requireSpotify: {
      async resolve({ resolveSpotifyUserId }) {
        const spotifyUserId = await resolveSpotifyUserId();
        if (!spotifyUserId) throw new AppError(401, "SPOTIFY_NOT_AUTHENTICATED", "Sessione Spotify richiesta");
        return { spotifyUserId };
      },
    },
  });
