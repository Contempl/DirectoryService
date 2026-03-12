"use client"

import type { LocationDto } from "@/entities/locations/types";
import LocationCard from "@/features/locations/location-card";
import { useCallback, useState } from "react";
import { useLocationsList } from "./model/use-locations-list";
import { Button } from "@/shared/components/ui/button";
import { EditLocationDialog } from "./edit-location-dialog";
import { Spinner } from "@/shared/components/ui/spinner";
import { CreateLocationDialog } from "./create-location-dialog";

export default function LocationsList() {

  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(5);
  const [isActive, setIsActive] = useState(true);
  const [createOpen, setCreateOpen] = useState(false)
  const [updateOpen, setUpdateOpen] = useState(false)

  const [selectedLocation, setSelectedLocation] = useState<LocationDto | undefined>(undefined);
 
  const { 
    data, 
    isPending, 
    isError, 
    error, 
    isFetchingNextPage,
    cursorRef
  } = useLocationsList(pageSize, isActive);

  

  if (isPending) return <p>Loading...</p>;

  if (isError)
    return (
      <p className="text-red-600">
        Error: {error instanceof Error ? error.message : "Ошибка"}
      </p>
    );

  if (!data || data.items.length === 0)
    return <p>No locations</p>;

const totalPages = Math.ceil(data.totalCount / pageSize);

  return (
    <div>
      <div className="flex justify-between items-center mb-4">
        <h2 className="text-2xl font-semibold">
          Locations ({data.totalCount})
        </h2>

        {isPending && (
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
    
        
      {data.items.map((location) => (
        <LocationCard
          key={location.id}
          location={location}
          onEdit={() => {
            setSelectedLocation(location)
            setUpdateOpen(true)
          }}
        />
      ))}
    </div>

    

    {/* кнопка создания */}
    <div className="mt-6">
      <Button onClick={() => setCreateOpen(true)}>
        Создать локацию
      </Button>
    </div>

    {/* create dialog */}
    <CreateLocationDialog
      open={createOpen}
      onOpenChange={setCreateOpen}
    />

    {/* update dialog */}
    {selectedLocation && (
      <EditLocationDialog
        key={selectedLocation.id}
        location={selectedLocation}
        open={selectedLocation !== undefined && updateOpen}
        onOpenChange={setUpdateOpen}
      />
    )}

    <div ref={cursorRef} className="flex justify-center py-4">
      {isFetchingNextPage && <Spinner />}
      </div>
    </div>
  );
}