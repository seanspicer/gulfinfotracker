import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { ArticleCard } from '../components/ArticleCard'
import { SkeletonCard } from '../components/SkeletonCard'
import { TopicFilter } from '../components/TopicFilter'
import { CountryFilter } from '../components/CountryFilter'
import { SortFilter } from '../components/SortFilter'
import { useArticles } from '../hooks/useArticles'

export function FeedPage() {
  const { t } = useTranslation()
  const [page, setPage] = useState(1)
  const { data, isLoading, isError } = useArticles(page)

  return (
    <div className="max-w-2xl mx-auto px-4 py-4 space-y-4">
      <TopicFilter />
      <CountryFilter />
      <SortFilter />

      {isLoading && (
        <div className="space-y-3" data-testid="skeleton-list">
          {Array.from({ length: 5 }).map((_, i) => <SkeletonCard key={i} />)}
        </div>
      )}

      {isError && (
        <div className="text-center py-8 text-red-500">Failed to load articles.</div>
      )}

      {data && (
        <>
          {data.data.length === 0 ? (
            <div className="text-center py-8 text-gray-500">{t('noResults')}</div>
          ) : (
            <div className="space-y-3">
              {data.data.map(article => (
                <ArticleCard key={article.id} article={article} />
              ))}
            </div>
          )}

          {data.total > page * 20 && (
            <button
              onClick={() => setPage(p => p + 1)}
              className="w-full py-2 text-sm text-indigo-600 border border-indigo-200 rounded-lg hover:bg-indigo-50 transition-colors"
            >
              {t('loadMore')}
            </button>
          )}
        </>
      )}
    </div>
  )
}
