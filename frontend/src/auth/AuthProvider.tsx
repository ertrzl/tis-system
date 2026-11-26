import { useEffect, useState, type ReactNode } from "react";
import { api, SESSION_EXPIRED_EVENT, tokenStore } from "../api/client";
import type { CurrentUser, LoginRequest, TokenResponse } from "../api/types";
import { AuthContext } from "./AuthContext";

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<CurrentUser | null>(null);
  // Saxlanmış sessiya varsa, istifadəçini /me ilə yoxlayana qədər "yüklənir" sayılır.
  const [loading, setLoading] = useState(() => tokenStore.hasSession());

  useEffect(() => {
    if (!tokenStore.hasSession()) return;

    // Giriş tokeni bitibsə api() özü yenilənmə tokeni ilə yenisini alıb sorğunu təkrarlayır.
    api<CurrentUser>("/api/auth/me")
      .then(setUser)
      .catch(() => tokenStore.clear()) // sessiya tamam bitib
      .finally(() => setLoading(false));
  }, []);

  // Sorğu zamanı sessiyanın bitdiyi məlum olarsa (yenilənmə də uğursuz) istifadəçi çıxarılır, login səhifəsinə düşür.
  useEffect(() => {
    const onExpired = () => setUser(null);
    window.addEventListener(SESSION_EXPIRED_EVENT, onExpired);
    return () => window.removeEventListener(SESSION_EXPIRED_EVENT, onExpired);
  }, []);

  function startSession(response: TokenResponse) {
    tokenStore.set(response.accessToken, response.refreshToken);
    setUser(response.user);
  }

  async function login(request: LoginRequest) {
    startSession(await api<TokenResponse>("/api/auth/login", { method: "POST", body: request }));
  }

  function logout() {
    // Əvvəlcə yerli sessiya silinir (çıxış dərhal görünsün), sonra server yenilənmə tokenini ləğv edir.
    const refreshToken = tokenStore.getRefresh();
    tokenStore.clear();
    setUser(null);

    if (refreshToken) {
      api("/api/auth/logout", { method: "POST", body: { refreshToken } }).catch(() => {
        // Server əlçatmazdırsa da istifadəçi artıq çıxıb; token özü də müddəti bitəndə köhnələcək.
      });
    }
  }

  return <AuthContext.Provider value={{ user, loading, login, startSession, logout }}>{children}</AuthContext.Provider>;
}
