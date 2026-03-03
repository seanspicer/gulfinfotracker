import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { apiClient } from '../lib/apiClient'

export function useSources() {
  return useQuery({
    queryKey: ['sources'],
    queryFn: () => apiClient.getSources(),
  })
}

export function useTriggerPoll() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (id: string) => apiClient.triggerPoll(id),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['sources'] }),
  })
}
