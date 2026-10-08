import { eq } from "drizzle-orm";
import { db } from "../db/client";
import { spotifyLinks, users } from "../db/schema";
import { AppError } from "../lib/response";

const hashPassword = (password: string) => Bun.password.hash(password, { algorithm: "argon2id" });

/** True when a DB error is a PostgreSQL unique_violation (23505). */
export function isUniqueViolation(error: unknown): boolean {
  return typeof error === "object" && error !== null && "code" in error && (error as { code: unknown }).code === "23505";
}

export async function getLinkByUserId(userId: string) {
  const [link] = await db.select().from(spotifyLinks).where(eq(spotifyLinks.userId, userId));
  return link ?? null;
}

export async function getLinkBySpotifyUserId(spotifyUserId: string) {
  const [link] = await db.select().from(spotifyLinks).where(eq(spotifyLinks.spotifyUserId, spotifyUserId));
  return link ?? null;
}

export async function findUserByEmail(email: string) {
  const [user] = await db.select().from(users).where(eq(users.email, email.toLowerCase()));
  return user ?? null;
}

export async function getUserById(userId: string) {
  const [user] = await db.select().from(users).where(eq(users.id, userId));
  return user ?? null;
}

export async function registerUser(email: string, password: string) {
  const normalized = email.toLowerCase();
  try {
    const [user] = await db
      .insert(users)
      .values({ email: normalized, passwordHash: await hashPassword(password) })
      .returning({ id: users.id, email: users.email });
    return user!;
  } catch (error) {
    if (isUniqueViolation(error)) throw new AppError(409, "EMAIL_IN_USE", "Email già registrata");
    throw error;
  }
}

export async function verifyCredentials(email: string, password: string) {
  const user = await findUserByEmail(email);
  if (!user || !(await Bun.password.verify(password, user.passwordHash))) {
    throw new AppError(401, "INVALID_CREDENTIALS", "Credenziali non valide");
  }
  return user;
}

export async function changePassword(userId: string, currentPassword: string, newPassword: string) {
  const user = await getUserById(userId);
  if (!user || !(await Bun.password.verify(currentPassword, user.passwordHash))) {
    throw new AppError(401, "INVALID_CREDENTIALS", "Password attuale non corretta");
  }
  await db.update(users).set({ passwordHash: await hashPassword(newPassword) }).where(eq(users.id, userId));
}

export async function unlinkSpotify(userId: string) {
  const [link] = await db.delete(spotifyLinks).where(eq(spotifyLinks.userId, userId)).returning();
  return link ?? null;
}
