import type { ReactNode } from "react";
import { useTranslation } from "react-i18next";
import { Navigate } from "react-router-dom";
import { useAuth } from "../auth/useAuth";

// Giriş etməyən istifadəçini /login səhifəsinə göndərir.
export function ProtectedRoute({ children }: { children: ReactNode }) {
  const { user, loading } = useAuth();
  const { t } = useTranslation();

  if (loading) return <p className="center-text">{t("common.loading")}</p>;
  if (!user) return <Navigate to="/login" replace />;

  return <>{children}</>;
}
