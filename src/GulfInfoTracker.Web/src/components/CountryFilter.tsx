import { useTranslation } from 'react-i18next'
import { useSearchParams } from 'react-router-dom'

const COUNTRIES = ['UAE', 'SA', 'QA', 'BH'] as const

export function CountryFilter() {
  const { t } = useTranslation()
  const [searchParams, setSearchParams] = useSearchParams()
  const current = searchParams.get('country') || ''

  function select(country: string) {
    setSearchParams(prev => {
      const next = new URLSearchParams(prev)
      if (country) next.set('country', country)
      else next.delete('country')
      next.delete('page')
      return next
    })
  }

  return (
    <div className="flex gap-2 overflow-x-auto pb-1 scrollbar-hide">
      <button
        onClick={() => select('')}
        className={`flex-none px-3 py-1.5 rounded-full text-xs font-medium transition-colors ${
          !current ? 'bg-emerald-600 text-white' : 'bg-gray-100 text-gray-700 hover:bg-gray-200'
        }`}
      >
        {t('allCountries')}
      </button>
      {COUNTRIES.map(c => (
        <button
          key={c}
          onClick={() => select(c)}
          className={`flex-none px-3 py-1.5 rounded-full text-xs font-medium transition-colors ${
            current === c ? 'bg-emerald-600 text-white' : 'bg-gray-100 text-gray-700 hover:bg-gray-200'
          }`}
        >
          {t(`countries.${c}`)}
        </button>
      ))}
    </div>
  )
}
