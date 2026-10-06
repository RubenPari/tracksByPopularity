import { ApiResponse } from "./response";

type EtagContext = {
  request: Request;
  set: { headers: Record<string, string | number>; status?: number | string };
};

/**
 * Wraps data in ApiResponse with a weak ETag. When the client already holds it, answers 304 with no body.
 * Returns the typed body so Eden can infer the response.
 * Cache-Control is private/no-cache so browsers revalidate with If-None-Match.
 */
export function withEtag<T>({ request, set }: EtagContext, data: T): ApiResponse<T> {
  const body = ApiResponse.Ok(data);
  const etag = `W/"${Bun.hash(JSON.stringify(body)).toString(36)}"`;
  set.headers.etag = etag;
  set.headers["cache-control"] = "private, no-cache";
  if (request.headers.get("if-none-match") === etag) {
    set.status = 304;
    // Eden still needs the declared return type; body is omitted via 304.
    return null as unknown as ApiResponse<T>;
  }
  return body;
}
