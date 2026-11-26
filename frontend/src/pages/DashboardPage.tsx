import { useTranslation } from "react-i18next";
import { useNavigate } from "react-router-dom";
import { useAuth } from "../auth/useAuth";
import { LanguageSwitcher } from "../components/LanguageSwitcher";

const sections = ["students", "teachers", "courses", "grades", "schedule"];

export function DashboardPage() {
  const { t } = useTranslation();
  const { user, logout } = useAuth();
  const navigate = useNavigate();

  if (!user) return null; // ProtectedRoute bunu onsuz da yoxlayır

  function handleLogout() {
    logout();
    navigate("/login", { replace: true });
  }

  return (
    <div className="app-shell">
      <header className="topbar">
        <strong>{t("app.title")}</strong>
        <div className="topbar-right">
          <LanguageSwitcher />
          <button type="button" onClick={handleLogout}>
            {t("auth.logout")}
          </button>
        </div>
      </header>

      <main className="content">
        <h1>{t("dashboard.welcome", { name: `${user.firstName} ${user.lastName}` })}</h1>

        <dl className="card info">
          <dt>{t("dashboard.university")}</dt>
          <dd>{user.tenantName}</dd>
          <dt>{t("auth.email")}</dt>
          <dd>{user.email}</dd>
          <dt>{t("dashboard.role")}</dt>
          <dd>{t(`roles.${user.role}`)}</dd>
        </dl>

        <div className="grid">
          {sections.map((key) => (
            <div key={key} className="card tile">
              <h2>{t(`nav.${key}`)}</h2>
              <p className="muted">{t("dashboard.comingSoon")}</p>
            </div>
          ))}
        </div>
      </main>
    </div>
  );
}
