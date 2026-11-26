import { useEffect, useState, type FormEvent } from "react";
import { useTranslation } from "react-i18next";
import { api, ApiError } from "../api/client";
import type { PagedResult, UserItem } from "../api/types";
import { AppShell } from "../components/AppShell";

const PAGE_SIZE = 20;

// Admin: öz universitetinin istifadəçiləri (səhifələnmiş, axtarışlı). Server yalnız həmin universitetin qeydlərini qaytarır.
export function UsersPage() {
  const { t } = useTranslation();

  const [data, setData] = useState<PagedResult<UserItem> | null>(null);
  const [page, setPage] = useState(1);
  const [searchInput, setSearchInput] = useState("");
  const [search, setSearch] = useState(""); // tətbiq olunmuş axtarış (yazdıqca yox, göndərəndə dəyişir)
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const params = new URLSearchParams({ page: String(page), pageSize: String(PAGE_SIZE) });
    if (search) params.set("search", search);

    api<PagedResult<UserItem>>(`/api/users?${params}`)
      .then((result) => {
        setData(result);
        setError(null);
      })
      .catch((err) => setError(err instanceof ApiError ? err.message : t("auth.networkError")))
      .finally(() => setLoading(false));
  }, [page, search, t]);

  function handleSearch(event: FormEvent) {
    event.preventDefault();
    setLoading(true);
    setPage(1);
    setSearch(searchInput.trim());
  }

  return (
    <AppShell>
      <h1>{t("users.title")}</h1>

      <form className="search-row" onSubmit={handleSearch}>
        <input
          type="search"
          value={searchInput}
          onChange={(e) => setSearchInput(e.target.value)}
          placeholder={t("users.searchPlaceholder")}
          aria-label={t("users.searchPlaceholder")}
        />
        <button type="submit">{t("users.search")}</button>
      </form>

      {error && (
        <p className="error" role="alert">
          {error}
        </p>
      )}

      <section className="card table-card">
        {loading && !data ? (
          <p className="muted">{t("common.loading")}</p>
        ) : data && data.items.length === 0 ? (
          <p className="muted">{t("users.empty")}</p>
        ) : (
          data && (
            <table>
              <thead>
                <tr>
                  <th>{t("users.name")}</th>
                  <th>{t("auth.studentNumber")}</th>
                  <th>{t("auth.email")}</th>
                  <th>{t("dashboard.role")}</th>
                  <th>{t("users.status")}</th>
                </tr>
              </thead>
              <tbody>
                {data.items.map((u) => (
                  <tr key={u.id}>
                    <td>
                      {u.firstName} {u.lastName}
                    </td>
                    <td>{u.studentNumber ?? "—"}</td>
                    <td>{u.email}</td>
                    <td>{t(`roles.${u.role}`)}</td>
                    <td>
                      <span className={u.invitationPending ? "badge badge-wait" : "badge badge-ok"}>
                        {u.invitationPending ? t("users.pending") : t("users.active")}
                      </span>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          )
        )}
      </section>

      {data && data.totalCount > 0 && (
        <div className="pager">
          <span className="muted">{t("users.total", { count: data.totalCount })}</span>
          <div className="pager-buttons">
            <button type="button" disabled={page <= 1} onClick={() => setPage((p) => p - 1)}>
              {t("users.prev")}
            </button>
            <span>{t("users.page", { page: data.page, pages: Math.max(data.totalPages, 1) })}</span>
            <button type="button" disabled={!data.hasMore} onClick={() => setPage((p) => p + 1)}>
              {t("users.next")}
            </button>
          </div>
        </div>
      )}
    </AppShell>
  );
}
