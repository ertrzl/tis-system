import i18n from "../i18n";

const TOKEN_KEY = "tis_token";

export const tokenStore = {
  get: () => localStorage.getItem(TOKEN_KEY),
  set: (token: string) => localStorage.setItem(TOKEN_KEY, token),
  clear: () => localStorage.removeItem(TOKEN_KEY)
};

// API-dən gələn xəta. message artıq istifadəçinin dilindədir (server Accept-Language-ə görə yazır).
export class ApiError extends Error {
  status: number;

  constructor(status: number, message: string) {
    super(message);
    this.status = status;
  }
}

export async function api<T>(path: string, options: { method?: string; body?: unknown } = {}): Promise<T> {
  const headers: Record<string, string> = {
    "Content-Type": "application/json",
    "Accept-Language": i18n.resolvedLanguage ?? "az"
  };

  const token = tokenStore.get();
  if (token) headers.Authorization = `Bearer ${token}`;

  let response: Response;
  try {
    response = await fetch(path, {
      method: options.method ?? "GET",
      headers,
      body: options.body === undefined ? undefined : JSON.stringify(options.body)
    });
  } catch {
    // Server işləmir və ya şəbəkə yoxdur
    throw new ApiError(0, i18n.t("auth.networkError"));
  }

  if (!response.ok) {
    const data = await response.json().catch(() => null);
    throw new ApiError(response.status, data?.message ?? i18n.t("auth.networkError"));
  }

  return response.json() as Promise<T>;
}
