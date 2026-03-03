import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { getSources, triggerPoll } from "../api/client";

export function useSources() {
  return useQuery({
    queryKey: ["sources"],
    queryFn: getSources,
  });
}

export function useTriggerPoll() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (pluginId: string) => triggerPoll(pluginId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["sources"] });
    },
  });
}
