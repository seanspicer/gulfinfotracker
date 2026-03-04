import { useQuery } from '@tanstack/react-query'
import { useSearchParams } from 'react-router-dom'
import { apiClient } from '../lib/apiClient'

export function useArticles(page = 1) {
  const [searchParams] = useSearchParams()
  const topic   = searchParams.get('topic')   || undefined
  const country = searchParams.get('country') || undefined
  const q       = searchParams.get('q')       || undefined
  const sortBy  = searchParams.get('sortBy')  || undefined

  return useQuery({
    queryKey: ['articles', { topic, country, q, sortBy, page }],
    queryFn: () => apiClient.getArticles({ topic, country, q, sortBy, page, pageSize: 20 }),
    staleTime: 1000 * 60 * 2,
    refetchInterval: 1000 * 30,
  })
}
