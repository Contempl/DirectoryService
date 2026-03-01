"use client"

import { locationsApi } from "@/entities/locations/api";
import type { LocationDto } from "@/entities/locations/types";
import LocationCard from "@/features/lessons/location-card";
import { PagedResult } from "@/shared/api/types";
import { useQuery } from "@tanstack/react-query";
import { useState } from "react";

export default function LocationsList() {

  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(5);
  const [isActive, setIsActive] = useState(true);
  const { data, isLoading, isError, error } = useQuery<
    PagedResult<LocationDto>
  >({
    queryKey: ["locations", { page, page_size: pageSize, isActive }],
    queryFn: () =>
      locationsApi.getLocations({
        page,
        pageSize: pageSize,
        isActive: isActive,
      }),
      staleTime: 1000 * 60,
  });

  if (isLoading) return <p>Loading...</p>;

  if (isError)
    return (
      <p className="text-red-600">
        Error: {error instanceof Error ? error.message : "Ошибка"}
      </p>
    );

  if (!data || data.data.length === 0)
    return <p>No locations</p>;

const totalPages = Math.ceil(data.totalCount / pageSize);

  return (
    <div>
      <div className="flex justify-between items-center mb-4">
        <h2 className="text-2xl font-semibold">
          Locations ({data.totalCount})
        </h2>

        {isLoading && (
          <span className="text-sm text-gray-500">Updating...</span>
        )}
      </div>

      {/* 🔹 Фильтр */}
      <div className="mb-6 flex gap-2">
        <button
          onClick={() => {
            setPage(1);
            setIsActive(true);
          }}
          className={`px-4 py-2 rounded transition-colors ${
            isActive
              ? "bg-blue-600 text-white"
              : "bg-gray-200 text-gray-800 hover:bg-gray-300"
          }`}
        >
          Active
        </button>

        <button
          onClick={() => {
            setPage(1);
            setIsActive(false);
          }}
          className={`px-4 py-2 rounded transition-colors ${
            !isActive
              ? "bg-blue-600 text-white"
              : "bg-gray-200 text-gray-800 hover:bg-gray-300"
          }`}
        >
          Inactive
        </button>
      </div>

      {/* 🔹 Список */}
      <div className="grid gap-4 grid-cols-1 sm:grid-cols-2 lg:grid-cols-3">
        {data.data.map((loc) => (
          <LocationCard key={loc.id} location={loc} />
        ))}
      </div>

      {/* 🔹 Пагинация */}
      {totalPages > 1 && (
        <div className="mt-6 flex gap-2">
          {Array.from({ length: totalPages }, (_, i) => i + 1).map(
            (pageNum) => (
              <button
                key={pageNum}
                onClick={() => setPage(pageNum)}
                className={`px-3 py-1 rounded transition-colors ${
                  pageNum === page
                    ? "bg-blue-600 text-white"
                    : "bg-gray-200 text-gray-800 hover:bg-gray-300"
                }`}
              >
                {pageNum}
              </button>
            )
          )}
        </div>
      )}
    </div>
  );
}