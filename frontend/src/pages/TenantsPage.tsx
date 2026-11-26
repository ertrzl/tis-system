import { useEffect, useState, type FormEvent } from "react";
import { useTranslation } from "react-i18next";
import { api, ApiError } from "../api/client";
import type { CreateTenantRequest, Tenant } from "../api/types";
import { AppShell } from "../components/AppShell";

const emptyForm: CreateTenantRequest = { code: "", name: "", adminEmail: "", adminFirstName: "", adminLastName: "" };

// SuperAdmin: universitetlərin siyahısı, yeni universitet (+ ilk admin) yaratmaq, aktiv/passiv etmək.
export function TenantsPage() {
  const { t, i18n } = useTranslation();

  const [tenants, setTenants] = useState<Tenant[]>([]);
  const [loading, setLoading] = useState(true);
  const [form, setForm] = useState<CreateTenantRequest>(emptyForm);
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [notice, setNotice] = useState<string | null>(null);

  useEffect(() => {
    api<Tenant[]>("/api/tenants")
      .then(setTenants)
      .catch((err) => setError(err instanceof ApiError ? err.message : t("auth.networkError")))
      .finally(() => setLoading(false));
  }, [t]);

  function update(field: keyof CreateTenantRequest, value: string) {
    setForm((current) => ({ ...current, [field]: value }));
  }

  async function handleCreate(event: FormEvent) {
    event.preventDefault();
    setError(null);
    setNotice(null);
    setSubmitting(true);

    try {
      const created = await api<Tenant>("/api/tenants", { method: "POST", body: form });
      setTenants((current) => [...current, created].sort((a, b) => a.name.localeCompare(b.name)));
      setNotice(
        created.inviteEmailSent
          ? t("tenants.inviteSent", { email: form.adminEmail })
          : t("tenants.inviteNotSent", { email: form.adminEmail })
      );
      setForm(emptyForm);
    } catch (err) {
      setError(err instanceof ApiError ? err.message : t("auth.networkError"));
    } finally {
      setSubmitting(false);
    }
  }

  async function toggleStatus(tenant: Tenant) {
    // Passiv etmək universitetin bütün istifadəçilərini bağlayır: təsdiq soruşulur.
    if (tenant.isActive && !window.confirm(t("tenants.confirmDeactivate", { name: tenant.name }))) return;

    setError(null);
    try {
      const updated = await api<Tenant>(`/api/tenants/${tenant.id}/status`, {
        method: "PUT",
        body: { isActive: !tenant.isActive }
      });
      setTenants((current) => current.map((x) => (x.id === updated.id ? updated : x)));
    } catch (err) {
      setError(err instanceof ApiError ? err.message : t("auth.networkError"));
    }
  }

  return (
    <AppShell>
      <h1>{t("tenants.title")}</h1>

      {error && (
        <p className="error" role="alert">
          {error}
        </p>
      )}
      {notice && (
        <p className="notice" role="status">
          {notice}
        </p>
      )}

      <section className="card">
        <h2>{t("tenants.new")}</h2>
        <form className="form-grid" onSubmit={handleCreate}>
          <label>
            {t("tenants.code")}
            <input value={form.code} onChange={(e) => update("code", e.target.value)} placeholder="BDU" required />
          </label>
          <label>
            {t("tenants.name")}
            <input value={form.name} onChange={(e) => update("name", e.target.value)} required />
          </label>
          <label>
            {t("tenants.adminFirstName")}
            <input value={form.adminFirstName} onChange={(e) => update("adminFirstName", e.target.value)} required />
          </label>
          <label>
            {t("tenants.adminLastName")}
            <input value={form.adminLastName} onChange={(e) => update("adminLastName", e.target.value)} required />
          </label>
          <label className="span-2">
            {t("tenants.adminEmail")}
            <input type="email" value={form.adminEmail} onChange={(e) => update("adminEmail", e.target.value)} required />
          </label>
          <button type="submit" className="primary span-2" disabled={submitting}>
            {submitting ? t("common.saving") : t("tenants.create")}
          </button>
        </form>
      </section>

      <section className="card table-card">
        {loading ? (
          <p className="muted">{t("common.loading")}</p>
        ) : (
          <table>
            <thead>
              <tr>
                <th>{t("tenants.code")}</th>
                <th>{t("tenants.name")}</th>
                <th>{t("tenants.users")}</th>
                <th>{t("tenants.status")}</th>
                <th>{t("tenants.created")}</th>
                <th />
              </tr>
            </thead>
            <tbody>
              {tenants.map((tenant) => (
                <tr key={tenant.id}>
                  <td>
                    <strong>{tenant.code}</strong>
                  </td>
                  <td>
                    {tenant.name} {tenant.isPlatform && <span className="badge">{t("tenants.platform")}</span>}
                  </td>
                  <td>{tenant.userCount}</td>
                  <td>
                    <span className={tenant.isActive ? "badge badge-ok" : "badge badge-off"}>
                      {tenant.isActive ? t("tenants.active") : t("tenants.inactive")}
                    </span>
                  </td>
                  <td>{new Date(tenant.createdAt).toLocaleDateString(i18n.language)}</td>
                  <td className="actions">
                    {!tenant.isPlatform && (
                      <button type="button" onClick={() => toggleStatus(tenant)}>
                        {tenant.isActive ? t("tenants.deactivate") : t("tenants.activate")}
                      </button>
                    )}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </section>
    </AppShell>
  );
}
