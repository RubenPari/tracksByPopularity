import { drizzle } from "drizzle-orm/bun-sql";
import { migrate } from "drizzle-orm/bun-sql/migrator";
import { config } from "../config";
import * as schema from "./schema";

/** Drizzle client over Bun SQL, with the full schema for typed queries. */
export const db = drizzle({ connection: config.databaseUrl, schema });

/** Applies pending SQL migrations from `backend/drizzle` (called at process start). */
export async function runMigrations() {
  await migrate(db, { migrationsFolder: new URL("../../drizzle", import.meta.url).pathname });
}
