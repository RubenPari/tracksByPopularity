import { config } from "../config";

// AES-256-GCM key for Spotify tokens at rest, derived from JWT_SECRET (not stored separately).
const aesKey = crypto.subtle.importKey(
  "raw",
  new Uint8Array(new Bun.CryptoHasher("sha256").update(`token-encryption:${config.jwtSecret}`).digest()),
  "AES-GCM",
  false,
  ["encrypt", "decrypt"],
);

/**
 * Encrypts a UTF-8 string with AES-256-GCM.
 * Returns `iv.ciphertext` as base64url segments (12-byte random IV per call).
 */
export async function encrypt(plain: string): Promise<string> {
  const iv = crypto.getRandomValues(new Uint8Array(12));
  const cipher = await crypto.subtle.encrypt({ name: "AES-GCM", iv }, await aesKey, new TextEncoder().encode(plain));
  return `${Buffer.from(iv).toString("base64url")}.${Buffer.from(cipher).toString("base64url")}`;
}

/**
 * Decrypts a payload produced by `encrypt`.
 * Throws if the format is not `iv.ciphertext` or authentication fails.
 */
export async function decrypt(payload: string): Promise<string> {
  const [iv, data] = payload.split(".");
  if (!iv || !data) throw new Error("Malformed encrypted payload");
  const plain = await crypto.subtle.decrypt(
    { name: "AES-GCM", iv: Buffer.from(iv, "base64url") },
    await aesKey,
    Buffer.from(data, "base64url"),
  );
  return new TextDecoder().decode(plain);
}
