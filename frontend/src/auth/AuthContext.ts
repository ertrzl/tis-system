import { createContext } from "react";
import type { CurrentUser, LoginRequest, TokenResponse } from "../api/types";

export interface AuthContextValue {
  user: CurrentUser | null;
  loading: boolean; // səhifə açılanda token yoxlanılır
  login: (request: LoginRequest) => Promise<void>;
  // Server token cütü qaytaranda sessiyanı başladır (giriş və dəvət qəbulu üçün).
  startSession: (response: TokenResponse) => void;
  logout: () => void;
}

export const AuthContext = createContext<AuthContextValue | null>(null);
