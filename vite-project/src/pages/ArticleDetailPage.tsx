import { useParams, Link } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { useArticle } from "../hooks/useArticles";
import { TOPICS } from "../types/api";
import CredibilityBadge from "../components/CredibilityBadge";

export default function ArticleDetailPage() {
  const { id } = useParams<{ id: string }>();
  const { t, i18n } = useTranslation();
  const lang = i18n.language as "en" | "ar";
  const { data: article, isLoading, isError } = useArticle(id ?? "");

  if (isLoading) return <p className="text-gray-500">{t("loading")}</p>;
  if (isError || !article) return <p className="text-red-600">{t("error")}</p>;

  const headline =
    lang === "ar" && article.headlineAr ? article.headlineAr : article.headlineEn;
  const summary =
    lang === "ar" && article.summaryAr ? article.summaryAr : article.summaryEn;

  return (
    <div className="mx-auto max-w-3xl">
      <Link to="/" className="mb-4 inline-block text-sm text-sky-600 hover:underline">
        {t("back")}
      </Link>

      <h1 className="mb-3 text-2xl font-bold text-gray-900">{headline}</h1>

      <div className="mb-4 flex flex-wrap items-center gap-3 text-sm text-gray-600">
        <CredibilityBadge score={article.credibilityScore} />
        <span>
          {t("source")}: {article.pluginId}
        </span>
        <span className="rounded bg-sky-50 px-1.5 py-0.5 font-medium text-sky-700">
          {article.country}
        </span>
        <span>
          {t("published")}: {new Date(article.publishedAt).toLocaleDateString()}
        </span>
        <span>
          {t("ingested")}: {new Date(article.ingestedAt).toLocaleDateString()}
        </span>
      </div>

      {article.topicIds.length > 0 && (
        <div className="mb-4 flex flex-wrap gap-2">
          <span className="text-sm font-medium text-gray-700">{t("topics")}:</span>
          {article.topicIds.map((tid) => (
            <span
              key={tid}
              className="rounded bg-gray-100 px-2 py-0.5 text-xs text-gray-700"
            >
              {TOPICS[tid]?.[lang] ?? tid}
            </span>
          ))}
        </div>
      )}

      {summary && (
        <div className="mb-6 rounded-lg border bg-white p-4 text-gray-700">
          {summary}
        </div>
      )}

      {article.credibilityReasoning && (
        <div className="mb-6">
          <h2 className="mb-2 text-lg font-semibold text-gray-800">{t("reasoning")}</h2>
          <p className="rounded-lg border bg-gray-50 p-4 text-sm text-gray-700">
            {article.credibilityReasoning}
          </p>
        </div>
      )}

      <a
        href={article.sourceUrl}
        target="_blank"
        rel="noopener noreferrer"
        className="inline-block rounded bg-sky-600 px-4 py-2 text-sm font-medium text-white hover:bg-sky-700"
      >
        {t("readOriginal")}
      </a>
    </div>
  );
}
