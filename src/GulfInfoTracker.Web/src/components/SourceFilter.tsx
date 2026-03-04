import { useTranslation } from 'react-i18next'
import { useSearchParams } from 'react-router-dom'
import { useSources } from '../hooks/useSources'

export function SourceFilter() {
  const { t } = useTranslation()
  const { data: sources } = useSources()
  const [searchParams, setSearchParams] = useSearchParams()
  const selected = searchParams.getAll('sources')

  function toggle(pluginId: string) {
    setSearchParams(prev => {
      const next = new URLSearchParams(prev)
      next.delete('sources')
      const updated = selected.includes(pluginId)
        ? selected.filter(s => s !== pluginId)
        : [...selected, pluginId]
      updated.forEach(s => next.append('sources', s))
      next.delete('page')
      return next
    })
  }

  function clearAll() {
    setSearchParams(prev => {
      const next = new URLSearchParams(prev)
      next.delete('sources')
      next.delete('page')
      return next
    })
  }

  return (
    <div className="flex gap-2 overflow-x-auto pb-1 scrollbar-hide">
      <button
        onClick={clearAll}
        className={`flex-none px-3 py-1.5 rounded-full text-xs font-medium transition-colors ${
          selected.length === 0 ? 'bg-violet-600 text-white' : 'bg-gray-100 text-gray-700 hover:bg-gray-200'
        }`}
      >
        {t('allSources')}
      </button>
      {sources?.map(s => (
        <button
          key={s.pluginId}
          onClick={() => toggle(s.pluginId)}
          className={`flex-none px-3 py-1.5 rounded-full text-xs font-medium transition-colors ${
            selected.includes(s.pluginId) ? 'bg-violet-600 text-white' : 'bg-gray-100 text-gray-700 hover:bg-gray-200'
          }`}
        >
          {s.displayName}
        </button>
      ))}
    </div>
  )
}
