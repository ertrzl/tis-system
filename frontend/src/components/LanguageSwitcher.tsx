import { useTranslation } from "react-i18next";

const languages = [
  { code: "az", label: "AZ" },
  { code: "en", label: "EN" },
  { code: "ru", label: "RU" }
];

export function LanguageSwitcher() {
  const { t, i18n } = useTranslation();

  return (
    <div className="lang-switcher" role="group" aria-label={t("common.language")}>
      {languages.map((l) => (
        <button
          key={l.code}
          type="button"
          className={i18n.resolvedLanguage === l.code ? "active" : ""}
          aria-pressed={i18n.resolvedLanguage === l.code}
          onClick={() => i18n.changeLanguage(l.code)}
        >
          {l.label}
        </button>
      ))}
    </div>
  );
}
