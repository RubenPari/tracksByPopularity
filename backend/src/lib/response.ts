export type ApiResponse<T> = {
  success: boolean;
  data: T | null;
  message: string;
  error: string | null;
};

export const ApiResponse = {
  Ok<T>(data: T, message = "Operazione completata con successo"): ApiResponse<T> {
    return { success: true, data, message, error: null };
  },
  Fail(message: string, error: string): ApiResponse<null> {
    return { success: false, data: null, message, error };
  },
};

export class AppError extends Error {
  status: number;
  code: string;

  constructor(status: number, code: string, message: string) {
    super(message);
    this.status = status;
    this.code = code;
  }
}
