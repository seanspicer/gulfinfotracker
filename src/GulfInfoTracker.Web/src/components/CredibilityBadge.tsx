import { useTranslation } from 'react-i18next'

interface Props {
  score: number | null
}

function getTier(score: number | null): { label: string; className: string; key: string } {
  if (score === null) return { label: 'Scoring pending', className: 'bg-gray-100 text-gray-600', key: 'pending' }
  if (score >= 80)    return { label: 'Verified',        className: 'bg-green-100 text-green-800', key: 'verified' }
  if (score >= 60)    return { label: 'Credible',        className: 'bg-blue-100 text-blue-800', key: 'credible' }
  if (score >= 40)    return { label: 'Uncertain',       className: 'bg-amber-100 text-amber-800', key: 'uncertain' }
  return { label: 'Low', className: 'bg-red-100 text-red-800', key: 'low' }
}

export function CredibilityBadge({ score }: Props) {
  const { t } = useTranslation()
  const tier = getTier(score)

  return (
    <span className={`inline-flex items-center gap-1 px-2 py-0.5 rounded-full text-xs font-medium ${tier.className}`}>
      {score !== null && <span className="font-bold">{score}</span>}
      <span>{t(`credibility.${tier.key}`)}</span>
    </span>
  )
}
