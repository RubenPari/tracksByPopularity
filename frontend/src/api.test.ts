import { describe, expect, test } from "bun:test";
import { ApiError, fetchData, send } from "./api";

describe("send / fetchData", () => {
  test("send returns body on success", async () => {
    const body = { success: true, data: { ok: true }, message: "ok", error: null };
    await expect(send(Promise.resolve({ data: body, error: null }))).resolves.toEqual(body);
  });

  test("send throws ApiError with server message", async () => {
    const request = Promise.resolve({
      data: null,
      error: { status: 401, value: { message: "Autenticazione richiesta", error: "UNAUTHORIZED" } },
    });
    try {
      await send(request);
      throw new Error("expected send to throw");
    } catch (error) {
      expect(error).toBeInstanceOf(ApiError);
      expect(error as ApiError).toMatchObject({ message: "Autenticazione richiesta", status: 401 });
    }
  });

  test("send falls back when error value is not an object message", async () => {
    const request = Promise.resolve({
      data: null,
      error: { status: 500, value: "boom" },
    });
    await expect(send(request)).rejects.toMatchObject({
      message: "Errore di comunicazione con il server",
      status: 500,
    });
  });

  test("fetchData unwraps ApiResponse.data", async () => {
    const body = { success: true, data: { id: "1" }, message: "ok", error: null };
    await expect(fetchData(Promise.resolve({ data: body, error: null }))).resolves.toEqual({ id: "1" });
  });
});
