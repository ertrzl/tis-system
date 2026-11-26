import { useTranslation } from "react-i18next";
import { Link } from "react-router-dom";
import { useAuth } from "../auth/useAuth";
import { AppShell } from "../components/AppShell";

// Hələ hazır olmayan bölmələr (yer tutucu). Rola görə göstərilir.
const upcomingSections = ["students", "teachers", "courses", "grades", "schedule"];

export function DashboardPage() {
  const { t } = useTranslation();
  const { user } = useAuth();

  if (!user) return null; // ProtectedRoute bunu onsuz da yoxlayır

  return (
    <AppShell>
      <h1>{t("dashboard.welcome", { name: `${user.firstName} ${user.lastName}` })}</h1>

      <dl className="card info">
        <dt>{t("dashboard.university")}</dt>
        <dd>{user.tenantName}</dd>
        {user.studentNumber && (
          <>
            <dt>{t("auth.studentNumber")}</dt>
            <dd>{user.studentNumber}</dd>
          </>
        )}
        <dt>{t("auth.email")}</dt>
        <dd>{user.email}</dd>
        <dt>{t("dashboard.role")}</dt>
        <dd>{t(`roles.${user.role}`)}</dd>
      </dl>

      <div className="grid">
        {user.role === "SuperAdmin" && (
          <Link to="/tenants" className="card tile tile-link">
            <h2>{t("nav.tenants")}</h2>
            <p className="muted">{t("dashboard.tenantsHint")}</p>
          </Link>
        )}

        {user.role === "Admin" && (
          <Link to="/users" className="card tile tile-link">
            <h2>{t("nav.users")}</h2>
            <p className="muted">{t("dashboard.usersHint")}</p>
          </Link>
        )}

        {user.role !== "SuperAdmin" &&
          upcomingSections.map((key) => (
            <div key={key} className="card tile">
              <h2>{t(`nav.${key}`)}</h2>
              <p className="muted">{t("dashboard.comingSoon")}</p>
            </div>
          ))}
      </div>
    </AppShell>
  );
}
