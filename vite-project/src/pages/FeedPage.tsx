import { useState } from "react";
import { useTranslation } from "react-i18next";
import { useArticles } from "../hooks/useArticles";
import ArticleCard from "../components/ArticleCard";
import TopicFilter from "../components/TopicFilter";
import CountryFilter from "../components/CountryFilter";
import Pagination from "../components/Pagination";

export default function FeedPage() {
  const { t } = useTranslation();
  const [topic, setTopic] = useState("");
  const [country, setCountry] = useState("");
  const [q, setQ] = useState("");
  const [page, setPage] = useState(1);
  const pageSize = 20;

  const { data, isLoading, isError } = useArticles({
    topic: topic || undefined,
    country: country || undefined,
    q: q || undefined,
    page,
    pageSize,
  });

  return (
    <div>
      <div className="mb-6 flex flex-wrap items-center gap-3">
        <input
          type="text"
          value={q}
          onChange={(e) => {
            setQ(e.target.value);
            setPage(1);
          }}
          placeholder={t("search")}
          className="rounded border border-gray-300 px-3 py-2 text-sm"
          aria-label="search"
        />
        <TopicFilter
          value={topic}
          onChange={(v) => {
            setTopic(v);
            setPage(1);
          }}
        />
        <CountryFilter
          value={country}
          onChange={(v) => {
            setCountry(v);
            setPage(1);
          }}
        />
      </div>

      {isLoading && <p className="text-gray-500">{t("loading")}</p>}
      {isError && <p className="text-red-600">{t("error")}</p>}

      {data && data.data.length === 0 && (
        <p className="text-gray-500">{t("noArticles")}</p>
      )}

      <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
        {data?.data.map((article) => (
          <ArticleCard key={article.id} article={article} />
        ))}
      </div>

      {data && (
        <Pagination
          page={page}
          pageSize={pageSize}
          total={data.total}
          onPageChange={setPage}
        />
      )}
    </div>
  );
}
