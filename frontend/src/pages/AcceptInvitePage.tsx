import { useEffect, useState, type FormEvent } from "react";
import { useTranslation } from "react-i18next";
import { useNavigate, useSearchParams } from "react-router-dom";
import { api, ApiError } from "../api/client";
import type { InviteInfo, TokenResponse } from "../api/types";
import { useAuth } from "../auth/useAuth";
import { LanguageSwitcher } from "../components/LanguageSwitcher";

// E-poçtdakı dəvət bağlantısı: /accept-invite?token=...  İstifadəçi parolunu təyin edir və dərhal daxil olur.
export function AcceptInvitePage() {
  const { t } = useTranslation();
  const { startSession } = useAuth();
  const navigate = useNavigate();
  const [params] = useSearchParams();
  const token = params.get("token") ?? "";

  const [invite, setInvite] = useState<InviteInfo | null>(null);
  const [loading, setLoading] = useState(true);
  const [password, setPassword] = useState("");
  const [confirm, setConfirm] = useState("");
  const [error, setError] = useState<string | null>(null);
  const [submitting, setSubmitting] = useState(false);

  useEffect(() => {
    // Bağlantı etibarsız/bitmiş/istifadə olunubsa server aydın mesaj qaytarır (istifadəçinin dilində).
    api<InviteInfo>(`/api/auth/invite?token=${encodeURIComponent(token)}`)
      .then(setInvite)
      .catch((err) => setError(err instanceof ApiError ? err.message : t("auth.networkError")))
      .finally(() => setLoading(false));
  }, [token, t]);

  async function handleSubmit(event: FormEvent) {
    event.preventDefault();
    setError(null);

    if (password !== confirm) {
      setError(t("invite.mismatch"));
      return;
    }

    setSubmitting(true);
    try {
      const response = await api<TokenResponse>("/api/auth/accept-invite", { method: "POST", body: { token, password } });
      startSession(response);
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

      <div className="card login-card">
        <h1>{t("invite.title")}</h1>

        {loading && <p className="muted">{t("common.loading")}</p>}

        {!loading && !invite && (
          <p className="error" role="alert">
            {error}
          </p>
        )}

        {invite && (
          <form className="login-form" onSubmit={handleSubmit}>
            <p className="muted">{t("invite.welcome", { name: invite.firstName, tenant: invite.tenantName })}</p>
            <p className="muted small">
              {invite.studentNumber ? `${t("auth.studentNumber")}: ${invite.studentNumber} · ` : ""}
              {invite.email}
            </p>

            <label>
              {t("invite.password")}
              <input
                type="password"
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                autoComplete="new-password"
                required
              />
            </label>
            <label>
              {t("invite.confirm")}
              <input
                type="password"
                value={confirm}
                onChange={(e) => setConfirm(e.target.value)}
                autoComplete="new-password"
                required
              />
            </label>
            <p className="muted small">{t("invite.hint")}</p>

            {error && (
              <p className="error" role="alert">
                {error}
              </p>
            )}

            <button type="submit" className="primary" disabled={submitting}>
              {submitting ? t("common.saving") : t("invite.submit")}
            </button>
          </form>
        )}
      </div>
    </main>
  );
}
