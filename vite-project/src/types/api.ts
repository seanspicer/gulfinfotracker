export interface PagedResult<T> {
  data: T[];
  total: number;
  page: number;
  pageSize: number;
}

export interface ArticleListItem {
  id: string;
  pluginId: string;
  headlineEn: string;
  headlineAr: string | null;
  summaryEn: string | null;
  summaryAr: string | null;
  sourceUrl: string;
  publishedAt: string;
  country: string;
  credibilityScore: number | null;
  fullText: boolean;
  translated: boolean;
  topicIds: string[];
}

export interface ArticleDetail {
  id: string;
  pluginId: string;
  headlineEn: string;
  headlineAr: string | null;
  summaryEn: string | null;
  summaryAr: string | null;
  sourceUrl: string;
  publishedAt: string;
  ingestedAt: string;
  country: string;
  credibilityScore: number | null;
  credibilityReasoning: string | null;
  fullText: boolean;
  translated: boolean;
  topicIds: string[];
}

export interface SourceHealth {
  pluginId: string;
  displayName: string;
  lastPolledAt: string | null;
  articlesLast24h: number;
  lastError: string | null;
}

export interface ArticleFilters {
  topic?: string;
  country?: string;
  q?: string;
  page?: number;
  pageSize?: number;
}

export const TOPICS: Record<string, { en: string; ar: string }> = {
  T1: { en: "Politics & Government", ar: "\u0633\u064A\u0627\u0633\u0629 \u0648\u062D\u0643\u0648\u0645\u0629" },
  T2: { en: "Economy & Finance", ar: "\u0627\u0642\u062A\u0635\u0627\u062F \u0648\u0645\u0627\u0644\u064A\u0629" },
  T3: { en: "Energy & Oil", ar: "\u0637\u0627\u0642\u0629 \u0648\u0646\u0641\u0637" },
  T4: { en: "Business & Investment", ar: "\u0623\u0639\u0645\u0627\u0644 \u0648\u0627\u0633\u062A\u062B\u0645\u0627\u0631" },
  T5: { en: "Iran/Israel/US Conflict", ar: "\u0635\u0631\u0627\u0639 \u0625\u064A\u0631\u0627\u0646/\u0625\u0633\u0631\u0627\u0626\u064A\u0644/\u0623\u0645\u0631\u064A\u0643\u0627" },
};

export const COUNTRIES = ["UAE", "KSA", "Qatar", "Kuwait", "Bahrain", "Oman", "Iraq", "Iran"];
