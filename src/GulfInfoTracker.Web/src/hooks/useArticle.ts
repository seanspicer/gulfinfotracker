import { useQuery } from '@tanstack/react-query'
import { apiClient } from '../lib/apiClient'

export function useArticle(id: string) {
  return useQuery({
    queryKey: ['article', id],
    queryFn: () => apiClient.getArticle(id),
    enabled: !!id,
  })
}
