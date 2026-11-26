import { createContext } from "react";
import type { CurrentUser, LoginRequest } from "../api/types";

export interface AuthContextValue {
  user: CurrentUser | null;
  loading: boolean; // səhifə açılanda token yoxlanılır
  login: (request: LoginRequest) => Promise<void>;
  logout: () => void;
}

export const AuthContext = createContext<AuthContextValue | null>(null);
