import type {
  PagedResult,
  ArticleListItem,
  ArticleDetail,
  SourceHealth,
  ArticleFilters,
} from "../types/api";

const BASE = "/api";

async function fetchJson<T>(url: string, init?: RequestInit): Promise<T> {
  const res = await fetch(url, init);
  if (!res.ok) {
    throw new Error(`API error ${res.status}: ${res.statusText}`);
  }
  return res.json() as Promise<T>;
}

export function getArticles(
  filters: ArticleFilters = {},
): Promise<PagedResult<ArticleListItem>> {
  const params = new URLSearchParams();
  if (filters.topic) params.set("topic", filters.topic);
  if (filters.country) params.set("country", filters.country);
  if (filters.q) params.set("q", filters.q);
  if (filters.page) params.set("page", String(filters.page));
  if (filters.pageSize) params.set("pageSize", String(filters.pageSize));
  const qs = params.toString();
  return fetchJson<PagedResult<ArticleListItem>>(
    `${BASE}/articles${qs ? `?${qs}` : ""}`,
  );
}

export function getArticle(id: string): Promise<ArticleDetail> {
  return fetchJson<ArticleDetail>(`${BASE}/articles/${id}`);
}

export function getSources(): Promise<SourceHealth[]> {
  return fetchJson<SourceHealth[]>(`${BASE}/sources`);
}

export function triggerPoll(
  pluginId: string,
): Promise<{ message: string }> {
  return fetchJson<{ message: string }>(`${BASE}/sources/${pluginId}/poll`, {
    method: "POST",
  });
}
