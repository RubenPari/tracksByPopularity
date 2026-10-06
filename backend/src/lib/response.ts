/** Standard JSON envelope returned by every API route. */
export type ApiResponse<T> = {
  success: boolean;
  data: T | null;
  message: string;
  error: string | null;
};

/** Helpers for success/failure responses consumed by the frontend Eden client. */
export const ApiResponse = {
  Ok<T>(data: T, message = "Operazione completata con successo"): ApiResponse<T> {
    return { success: true, data, message, error: null };
  },
  Fail(message: string, error: string): ApiResponse<null> {
    return { success: false, data: null, message, error };
  },
};

/**
 * Thrown for expected domain failures; mapped by the global `onError` handler
 * to HTTP status + `ApiResponse.Fail(message, code)`.
 */
export class AppError extends Error {
  status: number;
  code: string;

  constructor(status: number, code: string, message: string) {
    super(message);
    this.status = status;
    this.code = code;
  }
}
