import { useTranslation } from 'react-i18next'
import { useSearchParams } from 'react-router-dom'
import { Search } from 'lucide-react'
import { useCallback } from 'react'

export function Header() {
  const { t, i18n } = useTranslation()
  const [searchParams, setSearchParams] = useSearchParams()
  const q = searchParams.get('q') || ''

  const toggleLanguage = useCallback(() => {
    const nextLang = i18n.language === 'ar' ? 'en' : 'ar'
    i18n.changeLanguage(nextLang)
    localStorage.setItem('i18nextLng', nextLang)
    document.documentElement.dir  = nextLang === 'ar' ? 'rtl' : 'ltr'
    document.documentElement.lang = nextLang
    if (nextLang === 'ar') {
      document.body.classList.add('font-arabic')
    } else {
      document.body.classList.remove('font-arabic')
    }
  }, [i18n])

  function handleSearch(value: string) {
    setSearchParams(prev => {
      const next = new URLSearchParams(prev)
      if (value) next.set('q', value)
      else next.delete('q')
      next.delete('page')
      return next
    })
  }

  return (
    <header className="sticky top-0 z-40 bg-white border-b border-gray-200 shadow-sm">
      <div className="max-w-2xl mx-auto px-4 py-3 flex items-center gap-3">
        <h1 className="text-base font-bold text-indigo-700 whitespace-nowrap">{t('appName')}</h1>
        <div className="flex-1 relative">
          <Search className="absolute start-2.5 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-400" />
          <input
            type="search"
            placeholder={t('search')}
            value={q}
            onChange={e => handleSearch(e.target.value)}
            className="w-full ps-8 pe-3 py-1.5 text-sm border border-gray-200 rounded-full bg-gray-50 focus:outline-none focus:border-indigo-400"
          />
        </div>
        <button
          onClick={toggleLanguage}
          className="flex-none text-xs font-bold text-indigo-600 border border-indigo-200 rounded-full px-2.5 py-1 hover:bg-indigo-50 transition-colors"
          aria-label="Toggle language"
        >
          {i18n.language === 'ar' ? 'EN' : 'AR'}
        </button>
      </div>
    </header>
  )
}
