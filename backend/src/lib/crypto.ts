import { config } from "../config";

// AES-256-GCM key for Spotify tokens at rest, derived from JWT_SECRET.
const aesKey = crypto.subtle.importKey(
  "raw",
  new Uint8Array(new Bun.CryptoHasher("sha256").update(`token-encryption:${config.jwtSecret}`).digest()),
  "AES-GCM",
  false,
  ["encrypt", "decrypt"],
);

export async function encrypt(plain: string): Promise<string> {
  const iv = crypto.getRandomValues(new Uint8Array(12));
  const cipher = await crypto.subtle.encrypt({ name: "AES-GCM", iv }, await aesKey, new TextEncoder().encode(plain));
  return `${Buffer.from(iv).toString("base64url")}.${Buffer.from(cipher).toString("base64url")}`;
}

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
