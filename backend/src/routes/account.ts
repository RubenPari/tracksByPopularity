import { Elysia, t } from "elysia";
import { ApiResponse, AppError } from "../lib/response";
import { cookieOptions, directSpotifyUserId, session } from "../plugins/session";
import {
  changePassword,
  getLinkByUserId,
  getUserById,
  registerUser,
  verifyCredentials,
} from "../services/account";
import { linkSpotify } from "../spotify/tokens";

const credentials = t.Object({
  email: t.String({ format: "email", maxLength: 255 }),
  password: t.String({ minLength: 8, maxLength: 128 }),
});

/**
 * Local account API: register/login (JWT cookie), password change, link an active Spotify session, profile, logout.
 * JWT lives in `access_token` cookie only (SameSite=Strict); Bearer header still accepted for non-browser clients.
 * Spotify linking needs a direct Spotify session, not an existing link.
 */
export const accountRoutes = new Elysia({ prefix: "/api/account", detail: { tags: ["Account"] } })
  .use(session)
  .post(
    "/register",
    async ({ body, set }) => {
      const user = await registerUser(body.email, body.password);
      set.status = 201;
      return ApiResponse.Ok(user, "Registrazione completata");
    },
    { body: credentials },
  )
  .post(
    "/login",
    async ({ body, jwt, cookie }) => {
      const user = await verifyCredentials(body.email, body.password);
      const token = await jwt.sign({ sub: user.id });
      cookie.access_token.set({ value: token, maxAge: 7 * 86_400, ...cookieOptions() });
      return ApiResponse.Ok({ user: { id: user.id, email: user.email } }, "Login effettuato");
    },
    { body: credentials },
  )
  .post(
    "/change-password",
    async ({ body, userId }) => {
      await changePassword(userId, body.currentPassword, body.newPassword);
      return ApiResponse.Ok(null, "Password aggiornata");
    },
    {
      requireUser: true,
      body: t.Object({
        currentPassword: t.String({ minLength: 1 }),
        newPassword: t.String({ minLength: 8, maxLength: 128 }),
      }),
    },
  )
  .post(
    "/link-spotify",
    async ({ userId, headers, cookie }) => {
      // Only a direct Spotify session counts here, not the account's existing link.
      const spotifyUserId = await directSpotifyUserId(headers, cookie);
      if (!spotifyUserId) throw new AppError(401, "SPOTIFY_NOT_AUTHENTICATED", "Effettua prima il login Spotify");
      await linkSpotify(userId, spotifyUserId);
      return ApiResponse.Ok({ spotifyUserId }, "Account Spotify collegato");
    },
    { requireUser: true },
  )
  .get(
    "/me",
    async ({ userId }) => {
      const user = await getUserById(userId);
      if (!user) throw new AppError(404, "USER_NOT_FOUND", "Utente non trovato");
      const link = await getLinkByUserId(userId);
      return ApiResponse.Ok({
        id: user.id,
        email: user.email,
        spotifyLinked: !!link,
        spotifyUserId: link?.spotifyUserId ?? null,
      });
    },
    { requireUser: true },
  )
  .post(
    "/logout",
    ({ cookie }) => {
      // Clears the local JWT only; Spotify sessions are left intact.
      cookie.access_token.remove();
      return ApiResponse.Ok(null, "Logout effettuato");
    },
    { requireUser: true },
  );
