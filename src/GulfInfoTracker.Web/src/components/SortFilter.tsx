import { useTranslation } from 'react-i18next'
import { useSearchParams } from 'react-router-dom'

const SORT_OPTIONS = ['newest', 'oldest', 'score'] as const
type SortOption = typeof SORT_OPTIONS[number]

export function SortFilter() {
  const { t } = useTranslation()
  const [searchParams, setSearchParams] = useSearchParams()
  const current = (searchParams.get('sortBy') || 'newest') as SortOption

  function select(sortBy: SortOption) {
    setSearchParams(prev => {
      const next = new URLSearchParams(prev)
      if (sortBy === 'newest') next.delete('sortBy')
      else next.set('sortBy', sortBy)
      next.delete('page')
      return next
    })
  }

  return (
    <div className="flex gap-2 overflow-x-auto pb-1 scrollbar-hide">
      {SORT_OPTIONS.map(opt => (
        <button
          key={opt}
          onClick={() => select(opt)}
          className={`flex-none px-3 py-1.5 rounded-full text-xs font-medium transition-colors ${
            current === opt ? 'bg-amber-500 text-white' : 'bg-gray-100 text-gray-700 hover:bg-gray-200'
          }`}
        >
          {t(`sort.${opt}`)}
        </button>
      ))}
    </div>
  )
}
