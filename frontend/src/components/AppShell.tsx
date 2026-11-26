import type { ReactNode } from "react";
import { useTranslation } from "react-i18next";
import { Link, useNavigate } from "react-router-dom";
import { useAuth } from "../auth/useAuth";
import { LanguageSwitcher } from "./LanguageSwitcher";

// Daxil olmuş istifadəçi səhifələrinin ortaq çərçivəsi: yuxarı panel (ad, dil, çıxış) və məzmun sahəsi.
export function AppShell({ children }: { children: ReactNode }) {
  const { t } = useTranslation();
  const { logout } = useAuth();
  const navigate = useNavigate();

  function handleLogout() {
    logout();
    navigate("/login", { replace: true });
  }

  return (
    <div className="app-shell">
      <header className="topbar">
        <Link to="/" className="brand">
          {t("app.title")}
        </Link>
        <div className="topbar-right">
          <LanguageSwitcher />
          <button type="button" onClick={handleLogout}>
            {t("auth.logout")}
          </button>
        </div>
      </header>

      <main className="content">{children}</main>
    </div>
  );
}
