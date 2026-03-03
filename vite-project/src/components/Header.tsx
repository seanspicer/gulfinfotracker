import { Link } from "react-router-dom";
import { useTranslation } from "react-i18next";

export default function Header() {
  const { t, i18n } = useTranslation();

  const toggleLang = () => {
    const next = i18n.language === "en" ? "ar" : "en";
    i18n.changeLanguage(next);
    document.documentElement.lang = next;
    document.documentElement.dir = next === "ar" ? "rtl" : "ltr";
  };

  return (
    <header className="bg-slate-800 text-white shadow">
      <div className="mx-auto flex max-w-7xl items-center justify-between px-4 py-3">
        <Link to="/" className="text-xl font-bold tracking-tight">
          {t("appTitle")}
        </Link>
        <nav className="flex items-center gap-4 text-sm">
          <Link to="/" className="hover:text-sky-300">
            {t("feed")}
          </Link>
          <Link to="/admin/sources" className="hover:text-sky-300">
            {t("admin")}
          </Link>
          <button
            onClick={toggleLang}
            className="rounded border border-white/30 px-2 py-1 text-xs hover:bg-white/10"
          >
            {i18n.language === "en" ? "AR" : "EN"}
          </button>
        </nav>
      </div>
    </header>
  );
}
