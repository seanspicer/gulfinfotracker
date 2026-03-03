import { Link } from "react-router-dom";
import { useTranslation } from "react-i18next";
import type { ArticleListItem } from "../types/api";
import { TOPICS } from "../types/api";
import CredibilityBadge from "./CredibilityBadge";

interface Props {
  article: ArticleListItem;
}

export default function ArticleCard({ article }: Props) {
  const { i18n } = useTranslation();
  const lang = i18n.language as "en" | "ar";

  const headline =
    lang === "ar" && article.headlineAr ? article.headlineAr : article.headlineEn;
  const summary =
    lang === "ar" && article.summaryAr ? article.summaryAr : article.summaryEn;

  return (
    <Link
      to={`/article/${article.id}`}
      className="block rounded-lg border border-gray-200 bg-white p-4 shadow-sm transition hover:shadow-md"
    >
      <div className="mb-2 flex items-start justify-between gap-2">
        <h3 className="text-base font-semibold leading-snug text-gray-900">
          {headline}
        </h3>
        <CredibilityBadge score={article.credibilityScore} />
      </div>

      {summary && (
        <p className="mb-3 line-clamp-2 text-sm text-gray-600">{summary}</p>
      )}

      <div className="flex flex-wrap items-center gap-2 text-xs text-gray-500">
        <span className="rounded bg-sky-50 px-1.5 py-0.5 font-medium text-sky-700">
          {article.country}
        </span>
        <span>{article.pluginId}</span>
        <span>{new Date(article.publishedAt).toLocaleDateString()}</span>
        {article.topicIds.map((tid) => (
          <span
            key={tid}
            className="rounded bg-gray-100 px-1.5 py-0.5 text-gray-600"
          >
            {TOPICS[tid]?.[lang] ?? tid}
          </span>
        ))}
      </div>
    </Link>
  );
}
