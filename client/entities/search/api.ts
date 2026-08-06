import { queryOptions } from "@tanstack/react-query";

import { apiClient } from "@/shared/api/axios-instance";
import type { Envelope } from "@/shared/api/envelope";
import type { SearchResultDto } from "./types";

export const searchApi = {
  search: async (q: string) => {
    const response = await apiClient.get<Envelope<SearchResultDto[]>>("/search", { params: { q } });
    return response.data.result ?? [];
  },
};

export const searchQueryOptions = (q: string) =>
  queryOptions({
    queryKey: ["global-search", q],
    queryFn: () => searchApi.search(q),
    enabled: q.length >= 2,
    staleTime: 30_000,
  });
