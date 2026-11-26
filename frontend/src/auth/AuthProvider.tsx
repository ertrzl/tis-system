import { useEffect, useState, type ReactNode } from "react";
import { api, tokenStore } from "../api/client";
import type { CurrentUser, LoginRequest, TokenResponse } from "../api/types";
import { AuthContext } from "./AuthContext";

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<CurrentUser | null>(null);
  // Token varsa, istifadəçini /me ilə yoxlayana qədər "yüklənir" sayılır.
  const [loading, setLoading] = useState(() => tokenStore.get() !== null);

  useEffect(() => {
    if (!tokenStore.get()) return;

    api<CurrentUser>("/api/auth/me")
      .then(setUser)
      .catch(() => tokenStore.clear()) // token köhnəlib və ya yanlışdır
      .finally(() => setLoading(false));
  }, []);

  async function login(request: LoginRequest) {
    const response = await api<TokenResponse>("/api/auth/login", { method: "POST", body: request });
    tokenStore.set(response.accessToken);
    setUser(response.user);
  }

  function logout() {
    tokenStore.clear();
    setUser(null);
  }

  return <AuthContext.Provider value={{ user, loading, login, logout }}>{children}</AuthContext.Provider>;
}
