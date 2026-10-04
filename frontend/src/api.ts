import { treaty } from "@elysiajs/eden";
import type { App } from "../../backend/src/app";

export const api = treaty<App>(import.meta.env.VITE_API_URL, {
  fetch: { credentials: "include" },
});

export class ApiError extends Error {
  status: number;

  constructor(message: string, status: number) {
    super(message);
    this.status = status;
  }
}

type EdenResult<T> = { data: T; error: null } | { data: null; error: { status: unknown; value: unknown } };
type Body<T> = Exclude<T, null | undefined>;
type Payload<T> = Body<T> extends { data: infer D } ? Exclude<D, null> : never;

/** Awaits an Eden call and returns the ApiResponse body, throwing ApiError with the server message on failure. */
export async function send<T>(request: Promise<EdenResult<T>>): Promise<Body<T>> {
  const { data, error } = await request;
  if (error) {
    const value = error.value as { message?: string } | string | undefined;
    const message = typeof value === "object" && value?.message ? value.message : "Errore di comunicazione con il server";
    throw new ApiError(message, Number(error.status) || 0);
  }
  return data as Body<T>;
}

/** Like `send`, but returns only the `data` field of the ApiResponse. */
export async function fetchData<T>(request: Promise<EdenResult<T>>): Promise<Payload<T>> {
  return (await send(request) as { data: Payload<T> }).data;
}
