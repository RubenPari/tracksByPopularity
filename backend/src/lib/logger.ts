import { appendFile, mkdir, readdir, unlink } from "node:fs/promises";
import { join } from "node:path";

const LOG_DIR = process.env.LOG_DIR ?? "logs";
const RETENTION_DAYS = 30;
const ready = mkdir(LOG_DIR, { recursive: true }).catch(() => {});

type Level = "debug" | "info" | "warn" | "error";

function write(level: Level, message: string, meta: Record<string, unknown> = {}) {
  const timestamp = new Date().toISOString();
  const line = JSON.stringify({ timestamp, level, message, ...meta }) + "\n";
  (level === "error" ? process.stderr : process.stdout).write(line);
  if (process.env.NODE_ENV === "test") return;
  ready
    .then(() => appendFile(join(LOG_DIR, `app-${timestamp.slice(0, 10)}.log`), line))
    .catch(() => {});
}

export const logger = {
  debug: (message: string, meta?: Record<string, unknown>) => write("debug", message, meta),
  info: (message: string, meta?: Record<string, unknown>) => write("info", message, meta),
  warn: (message: string, meta?: Record<string, unknown>) => write("warn", message, meta),
  error: (message: string, meta?: Record<string, unknown>) => write("error", message, meta),
};

/** Deletes daily log files older than RETENTION_DAYS. */
export async function pruneLogs() {
  const cutoff = new Date(Date.now() - RETENTION_DAYS * 86_400_000).toISOString().slice(0, 10);
  for (const file of await readdir(LOG_DIR).catch(() => [] as string[])) {
    const day = file.match(/^app-(\d{4}-\d{2}-\d{2})\.log$/)?.[1];
    if (day && day < cutoff) await unlink(join(LOG_DIR, file)).catch(() => {});
  }
}
