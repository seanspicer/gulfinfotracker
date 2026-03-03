import { useTranslation } from 'react-i18next'
import { useSearchParams } from 'react-router-dom'

const TOPICS = ['T1', 'T2', 'T3', 'T4', 'T5'] as const

export function TopicFilter() {
  const { t } = useTranslation()
  const [searchParams, setSearchParams] = useSearchParams()
  const current = searchParams.get('topic') || ''

  function select(topicId: string) {
    setSearchParams(prev => {
      const next = new URLSearchParams(prev)
      if (topicId) next.set('topic', topicId)
      else next.delete('topic')
      next.delete('page')
      return next
    })
  }

  return (
    <div className="flex gap-2 overflow-x-auto pb-1 scrollbar-hide">
      <button
        onClick={() => select('')}
        className={`flex-none px-3 py-1.5 rounded-full text-xs font-medium transition-colors ${
          !current ? 'bg-indigo-600 text-white' : 'bg-gray-100 text-gray-700 hover:bg-gray-200'
        }`}
      >
        {t('allTopics')}
      </button>
      {TOPICS.map(tid => (
        <button
          key={tid}
          onClick={() => select(tid)}
          className={`flex-none px-3 py-1.5 rounded-full text-xs font-medium transition-colors ${
            current === tid ? 'bg-indigo-600 text-white' : 'bg-gray-100 text-gray-700 hover:bg-gray-200'
          }`}
        >
          {t(`topics.${tid}`)}
        </button>
      ))}
    </div>
  )
}
