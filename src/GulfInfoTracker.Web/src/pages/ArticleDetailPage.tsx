import { useParams, Link } from 'react-router-dom'
import { useTranslation } from 'react-i18next'
import { ArrowLeft, ExternalLink } from 'lucide-react'
import { CredibilityBadge } from '../components/CredibilityBadge'
import { useArticle } from '../hooks/useArticle'

export function ArticleDetailPage() {
  const { id } = useParams<{ id: string }>()
  const { t, i18n } = useTranslation()
  const { data: article, isLoading, isError } = useArticle(id!)

  if (isLoading) return <div className="p-4 text-center text-gray-500">Loading...</div>
  if (isError || !article) return <div className="p-4 text-center text-red-500">Article not found.</div>

  const isAr = i18n.language === 'ar'
  const headline = (isAr && article.headlineAr) ? article.headlineAr : article.headlineEn
  const summary  = (isAr && article.summaryAr)  ? article.summaryAr  : article.summaryEn

  return (
    <div className="max-w-2xl mx-auto px-4 py-4 space-y-4">
      <Link to="/" className="inline-flex items-center gap-1 text-sm text-indigo-600 hover:underline">
        <ArrowLeft className="w-4 h-4" />
        {t('back')}
      </Link>

      <article className="bg-white rounded-lg shadow-sm p-5 space-y-4">
        <h1 className="text-lg font-bold text-gray-900 leading-snug">{headline}</h1>

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

        {summary && (
          <p className="text-sm text-gray-700 leading-relaxed">{summary}</p>
        )}

        {article.translated && (
          <p className="text-xs text-amber-600 italic">{t('machineTranslated')}</p>
        )}

        <div className="border-t border-gray-100 pt-4 space-y-2">
          <div className="flex items-center gap-3">
            <span className="text-sm font-medium text-gray-700">Credibility Score</span>
            <CredibilityBadge score={article.credibilityScore} />
          </div>
          {article.credibilityReasoning && (
            <p className="text-sm text-gray-600 leading-relaxed">{article.credibilityReasoning}</p>
          )}
        </div>

        <a
          href={article.sourceUrl}
          target="_blank"
          rel="noopener noreferrer"
          className="inline-flex items-center gap-1 text-sm text-indigo-600 hover:underline"
        >
          <ExternalLink className="w-4 h-4" />
          {t('viewOriginal')}
        </a>
      </article>
    </div>
  )
}
