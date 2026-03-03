const BASE_URL = (import.meta.env.VITE_API_URL as string) || ''

export interface ArticleListItem {
  id: string
  pluginId: string
  headlineEn: string
  headlineAr: string | null
  summaryEn: string | null
  summaryAr: string | null
  sourceUrl: string
  publishedAt: string
  country: string
  credibilityScore: number | null
  fullText: boolean
  translated: boolean
  topicIds: string[]
}

export interface ArticleDetail extends ArticleListItem {
  ingestedAt: string
  credibilityReasoning: string | null
}

export interface PagedResult<T> {
  data: T[]
  total: number
  page: number
  pageSize: number
}

export interface SourceHealth {
  pluginId: string
  displayName: string
  lastPolledAt: string | null
  articlesLast24h: number
  lastError: string | null
}

async function apiFetch<T>(path: string): Promise<T> {
  const res = await fetch(`${BASE_URL}${path}`)
  if (!res.ok) throw new Error(`HTTP ${res.status}: ${path}`)
  return res.json() as Promise<T>
}

export const apiClient = {
  getArticles: (params: {
    topic?: string
    country?: string
    q?: string
    page?: number
    pageSize?: number
  }) => {
    const sp = new URLSearchParams()
    if (params.topic)   sp.set('topic',    params.topic)
    if (params.country) sp.set('country',  params.country)
    if (params.q)       sp.set('q',        params.q)
    if (params.page)    sp.set('page',     String(params.page))
    if (params.pageSize) sp.set('pageSize', String(params.pageSize))
    return apiFetch<PagedResult<ArticleListItem>>(`/api/articles?${sp}`)
  },

  getArticle: (id: string) =>
    apiFetch<ArticleDetail>(`/api/articles/${id}`),

  getSources: () =>
    apiFetch<SourceHealth[]>('/api/sources'),

  triggerPoll: async (id: string) => {
    const res = await fetch(`${BASE_URL}/api/sources/${id}/poll`, { method: 'POST' })
    if (!res.ok) throw new Error(`HTTP ${res.status}`)
    return res.json()
  },
}
