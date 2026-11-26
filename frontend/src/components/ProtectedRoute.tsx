import type { ReactNode } from "react";
import { useTranslation } from "react-i18next";
import { Navigate } from "react-router-dom";
import type { UserRole } from "../api/types";
import { useAuth } from "../auth/useAuth";

// Giriş etməyən istifadəçini /login səhifəsinə göndərir. "roles" verilibsə, rolu uyğun olmayanı ana səhifəyə qaytarır
// (server də eyni yoxlamanı edir: bu yalnız istifadəçiyə lazımsız səhifə göstərməmək üçündür, təhlükəsizlik qatı deyil).
export function ProtectedRoute({ children, roles }: { children: ReactNode; roles?: UserRole[] }) {
  const { user, loading } = useAuth();
  const { t } = useTranslation();

  if (loading) return <p className="center-text">{t("common.loading")}</p>;
  if (!user) return <Navigate to="/login" replace />;
  if (roles && !roles.includes(user.role)) return <Navigate to="/" replace />;

  return <>{children}</>;
}
