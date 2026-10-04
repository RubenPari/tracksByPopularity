import { ApiResponse } from "./response";

/** Wraps data in ApiResponse with a weak ETag; answers 304 when the client already has it. */
export function withEtag<T>(request: Request, data: T) {
  const body = ApiResponse.Ok(data);
  const json = JSON.stringify(body);
  const etag = `W/"${Bun.hash(json).toString(36)}"`;
  const headers = { ETag: etag, "Cache-Control": "private, no-cache", "Content-Type": "application/json" };
  if (request.headers.get("if-none-match") === etag) return new Response(null, { status: 304, headers });
  return new Response(json, { headers });
}
