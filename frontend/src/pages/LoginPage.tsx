import { useState, type FormEvent } from "react";
import { useTranslation } from "react-i18next";
import { Navigate, useNavigate } from "react-router-dom";
import { ApiError } from "../api/client";
import { useAuth } from "../auth/useAuth";
import { LanguageSwitcher } from "../components/LanguageSwitcher";

const TENANT_KEY = "tis_tenant";

export function LoginPage() {
  const { t } = useTranslation();
  const { user, login } = useAuth();
  const navigate = useNavigate();

  // Universitet kodu yadda qalır ki, hər dəfə yazmaq lazım olmasın.
  const [tenantCode, setTenantCode] = useState(() => localStorage.getItem(TENANT_KEY) ?? "");
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState<string | null>(null);
  const [submitting, setSubmitting] = useState(false);

  if (user) return <Navigate to="/" replace />;

  async function handleSubmit(event: FormEvent) {
    event.preventDefault();
    setError(null);
    setSubmitting(true);

    try {
      const code = tenantCode.trim().toUpperCase();
      await login({ tenantCode: code, email: email.trim(), password });
      localStorage.setItem(TENANT_KEY, code);
      navigate("/", { replace: true });
    } catch (err) {
      setError(err instanceof ApiError ? err.message : t("auth.networkError"));
    } finally {
      setSubmitting(false);
    }
  }

  return (
    <main className="login-page">
      <div className="login-lang">
        <LanguageSwitcher />
      </div>

      <form className="card login-card" onSubmit={handleSubmit}>
        <h1>{t("app.title")}</h1>
        <p className="muted">{t("auth.loginSubtitle")}</p>

        <label>
          {t("auth.tenantCode")}
          <input
            value={tenantCode}
            onChange={(e) => setTenantCode(e.target.value)}
            placeholder="BDU"
            autoComplete="organization"
            required
          />
        </label>

        <label>
          {t("auth.email")}
          <input type="email" value={email} onChange={(e) => setEmail(e.target.value)} autoComplete="username" required />
        </label>

        <label>
          {t("auth.password")}
          <input
            type="password"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            autoComplete="current-password"
            required
          />
        </label>

        {error && (
          <p className="error" role="alert">
            {error}
          </p>
        )}

        <button type="submit" className="primary" disabled={submitting}>
          {submitting ? t("auth.signingIn") : t("auth.login")}
        </button>
      </form>
    </main>
  );
}
