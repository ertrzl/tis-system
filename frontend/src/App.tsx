import { useTranslation } from "react-i18next";

const languages = [
  { code: "az", label: "AZ" },
  { code: "en", label: "EN" },
  { code: "ru", label: "RU" }
];

function App() {
  const { t, i18n } = useTranslation();

  return (
    <main style={{ padding: 32 }}>
      <div style={{ display: "flex", gap: 8, justifyContent: "flex-end" }}>
        {languages.map((l) => (
          <button key={l.code} disabled={i18n.resolvedLanguage === l.code} onClick={() => i18n.changeLanguage(l.code)}>
            {l.label}
          </button>
        ))}
      </div>
      <h1>{t("app.title")}</h1>
      <p>{t("app.subtitle")}</p>
      <nav style={{ display: "flex", gap: 16 }}>
        {["dashboard", "students", "teachers", "courses", "grades", "schedule"].map((k) => (
          <span key={k}>{t(`nav.${k}`)}</span>
        ))}
      </nav>
    </main>
  );
}

export default App;
