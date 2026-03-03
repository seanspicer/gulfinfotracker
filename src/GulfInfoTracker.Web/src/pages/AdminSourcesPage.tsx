import { useTranslation } from 'react-i18next'
import { useSources, useTriggerPoll } from '../hooks/useSources'

export function AdminSourcesPage() {
  const { t } = useTranslation()
  const { data: sources, isLoading } = useSources()
  const { mutate: triggerPoll, isPending } = useTriggerPoll()

  if (isLoading) return <div className="p-4 text-center text-gray-500">Loading...</div>

  return (
    <div className="max-w-4xl mx-auto px-4 py-6">
      <h1 className="text-xl font-bold text-gray-900 mb-4">{t('adminSources')}</h1>
      <div className="overflow-x-auto">
        <table className="w-full text-sm">
          <thead>
            <tr className="border-b border-gray-200 text-left text-xs text-gray-500 uppercase">
              <th className="pb-2 pe-4">Plugin</th>
              <th className="pb-2 pe-4">{t('lastPolled')}</th>
              <th className="pb-2 pe-4">{t('articles24h')}</th>
              <th className="pb-2 pe-4">{t('lastError')}</th>
              <th className="pb-2" />
            </tr>
          </thead>
          <tbody className="divide-y divide-gray-100">
            {sources?.map(source => (
              <tr key={source.pluginId} className="py-2">
                <td className="py-3 pe-4">
                  <div className="font-medium text-gray-900">{source.displayName}</div>
                  <div className="text-xs text-gray-500">{source.pluginId}</div>
                </td>
                <td className="py-3 pe-4 text-gray-600">
                  {source.lastPolledAt
                    ? new Date(source.lastPolledAt).toLocaleString()
                    : '—'}
                </td>
                <td className="py-3 pe-4 text-gray-700 font-medium">{source.articlesLast24h}</td>
                <td className="py-3 pe-4">
                  {source.lastError ? (
                    <span className="text-red-600 text-xs">{source.lastError.slice(0, 60)}</span>
                  ) : (
                    <span className="text-green-600 text-xs">OK</span>
                  )}
                </td>
                <td className="py-3">
                  <button
                    onClick={() => triggerPoll(source.pluginId)}
                    disabled={isPending}
                    className="text-xs text-indigo-600 border border-indigo-200 rounded px-2 py-1 hover:bg-indigo-50 disabled:opacity-50"
                  >
                    {t('triggerPoll')}
                  </button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  )
}
