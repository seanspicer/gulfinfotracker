import { useQuery } from "@tanstack/react-query";
import { getArticles, getArticle } from "../api/client";
import type { ArticleFilters } from "../types/api";

export function useArticles(filters: ArticleFilters = {}) {
  return useQuery({
    queryKey: ["articles", filters],
    queryFn: () => getArticles(filters),
  });
}

export function useArticle(id: string) {
  return useQuery({
    queryKey: ["article", id],
    queryFn: () => getArticle(id),
    enabled: !!id,
  });
}
