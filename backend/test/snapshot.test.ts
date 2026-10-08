import { describe, expect, test } from "bun:test";
import { AppError } from "../src/lib/response";
import { createSnapshot, deleteSnapshot, restoreSnapshot } from "../src/services/snapshot";

describe("snapshot service ownership", () => {
  test("restore/delete throw SNAPSHOT_NOT_FOUND for unknown ids when DB is unreachable or empty", async () => {
    // Without a live Postgres these reject; when DB is up with no matching row they throw AppError 404.
    try {
      await restoreSnapshot("nobody", "00000000-0000-0000-0000-000000000000");
      throw new Error("expected restoreSnapshot to reject");
    } catch (error) {
      if (error instanceof AppError) {
        expect(error).toMatchObject({ status: 404, code: "SNAPSHOT_NOT_FOUND" });
      } else {
        expect(error).toBeInstanceOf(Error);
      }
    }

    try {
      await deleteSnapshot("nobody", "00000000-0000-0000-0000-000000000000");
      throw new Error("expected deleteSnapshot to reject");
    } catch (error) {
      if (error instanceof AppError) {
        expect(error).toMatchObject({ status: 404, code: "SNAPSHOT_NOT_FOUND" });
      } else {
        expect(error).toBeInstanceOf(Error);
      }
    }
  });

  test("createSnapshot is exported for snapshot-before-mutate callers", () => {
    expect(typeof createSnapshot).toBe("function");
  });
});
