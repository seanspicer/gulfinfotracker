import { Link } from 'react-router-dom'
import { useTranslation } from 'react-i18next'
import { CredibilityBadge } from './CredibilityBadge'
import type { ArticleListItem } from '../lib/apiClient'

interface Props {
  article: ArticleListItem
}

export function ArticleCard({ article }: Props) {
  const { t, i18n } = useTranslation()
  const isAr = i18n.language === 'ar'

  const headline = (isAr && article.headlineAr) ? article.headlineAr : article.headlineEn
  const summary  = (isAr && article.summaryAr)  ? article.summaryAr  : article.summaryEn

  return (
    <Link to={`/articles/${article.id}`} className="block bg-white rounded-lg shadow-sm hover:shadow-md transition-shadow p-4">
      <div className="flex items-start justify-between gap-3 mb-2">
        <h2 className="text-sm font-semibold text-gray-900 leading-snug flex-1">{headline}</h2>
        <CredibilityBadge score={article.credibilityScore} />
      </div>

      {summary && (
        <p className="text-xs text-gray-600 line-clamp-2 mb-3">{summary}</p>
      )}

      <div className="flex items-center gap-2 flex-wrap text-xs text-gray-500">
        <span className="font-medium text-gray-700">{article.pluginId}</span>
        <span>·</span>
        <span>{t(`countries.${article.country}`, { defaultValue: article.country })}</span>
        <span>·</span>
        <span>{new Date(article.publishedAt).toLocaleDateString(i18n.language)}</span>
        {article.topicIds.map(tid => (
          <span key={tid} className="bg-indigo-50 text-indigo-700 px-1.5 py-0.5 rounded">
            {t(`topics.${tid}`, { defaultValue: tid })}
          </span>
        ))}
      </div>
    </Link>
  )
}
