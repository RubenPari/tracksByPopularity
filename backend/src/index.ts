import { app } from "./app";
import { config } from "./config";
import { runMigrations } from "./db/client";
import { jobs } from "./jobs";
import { logger } from "./lib/logger";

await runMigrations();
app.use(jobs).listen(config.port);
logger.info("server started", { port: config.port });

export type { App } from "./app";
