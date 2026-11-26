import i18n from "../i18n";
import type { TokenResponse } from "./types";

const ACCESS_KEY = "tis_token";
const REFRESH_KEY = "tis_refresh";

// Giriş tokeni qısa yaşayır (15 dəq), yenilənmə tokeni uzun (30 gün). Giriş tokeni bitəndə yenilənmə tokeni ilə
// səssizcə yenisi alınır, istifadəçi parolu yenidən yazmır.
export const tokenStore = {
  getAccess: () => localStorage.getItem(ACCESS_KEY),
  getRefresh: () => localStorage.getItem(REFRESH_KEY),
  set: (access: string, refresh: string) => {
    localStorage.setItem(ACCESS_KEY, access);
    localStorage.setItem(REFRESH_KEY, refresh);
  },
  clear: () => {
    localStorage.removeItem(ACCESS_KEY);
    localStorage.removeItem(REFRESH_KEY);
  },
  hasSession: () => localStorage.getItem(REFRESH_KEY) !== null
};

// Sessiya bitəndə (yenilənmə də uğursuz olanda) AuthProvider bu hadisəni dinləyib istifadəçini çıxarır.
export const SESSION_EXPIRED_EVENT = "tis:session-expired";

// API-dən gələn xəta. message artıq istifadəçinin dilindədir (server Accept-Language-ə görə yazır).
export class ApiError extends Error {
  status: number;

  constructor(status: number, message: string) {
    super(message);
    this.status = status;
  }
}

interface RequestOptions {
  method?: string;
  body?: unknown;
}

async function send(path: string, options: RequestOptions, withToken: boolean): Promise<Response> {
  const headers: Record<string, string> = {
    "Content-Type": "application/json",
    "Accept-Language": i18n.resolvedLanguage ?? "az"
  };

  const token = tokenStore.getAccess();
  if (withToken && token) headers.Authorization = `Bearer ${token}`;

  try {
    return await fetch(path, {
      method: options.method ?? "GET",
      headers,
      body: options.body === undefined ? undefined : JSON.stringify(options.body)
    });
  } catch {
    // Server işləmir və ya şəbəkə yoxdur
    throw new ApiError(0, i18n.t("auth.networkError"));
  }
}

// Eyni anda bir neçə sorğu 401 alsa yenilənmə yalnız BİR dəfə göndərilir. Yenilənmə tokeni birdəfəlikdir,
// ikinci dəfə göndərilsə server onu oğurlanmış sayıb bütün sessiyaları bağlayardı.
let refreshing: Promise<boolean> | null = null;

function refreshTokens(): Promise<boolean> {
  refreshing ??= doRefresh().finally(() => {
    refreshing = null;
  });
  return refreshing;
}

async function doRefresh(): Promise<boolean> {
  const refreshToken = tokenStore.getRefresh();
  if (!refreshToken) return false;

  const response = await send("/api/auth/refresh", { method: "POST", body: { refreshToken } }, false);
  if (!response.ok) {
    tokenStore.clear();
    return false;
  }

  const data = (await response.json()) as TokenResponse;
  tokenStore.set(data.accessToken, data.refreshToken);
  return true;
}

// Giriş, yenilənmə və çıxış sorğuları özləri tokensiz işləyir: 401-də yenidən yeniləməyə çalışmaq mənasızdır.
function canRefresh(path: string): boolean {
  return tokenStore.hasSession() && !path.startsWith("/api/auth/login") && !path.startsWith("/api/auth/refresh") && !path.startsWith("/api/auth/logout");
}

export async function api<T>(path: string, options: RequestOptions = {}): Promise<T> {
  let response = await send(path, options, true);

  if (response.status === 401 && canRefresh(path)) {
    if (await refreshTokens()) {
      response = await send(path, options, true);
    } else {
      window.dispatchEvent(new Event(SESSION_EXPIRED_EVENT));
    }
  }

  if (!response.ok) {
    const data = await response.json().catch(() => null);
    throw new ApiError(response.status, data?.message ?? i18n.t("auth.networkError"));
  }

  // 204 No Content (məs: çıxış) gövdəsizdir
  if (response.status === 204) return undefined as T;

  return response.json() as Promise<T>;
}
