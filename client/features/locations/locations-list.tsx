"use client"

import type { LocationDto } from "@/entities/locations/types";
import LocationCard from "@/features/locations/location-card";
import { useState } from "react";
import { useLocationsList } from "./model/use-locations-list";
import { Button } from "@/shared/components/ui/button";
import { EditLocationDialog } from "./edit-location-dialog";
import { Spinner } from "@/shared/components/ui/spinner";
import { CreateLocationDialog } from "./create-location-dialog";
import { useGetLocationsFilter } from "./model/locations-filter-store";
import { LocationsFilter } from "./locations-filter";
import { DepartmentsPicker } from "../departments/departments-picker";
import { useLocationDepartmentFilter } from "./model/use-location-department-filter";
import { useLocationsView } from "./model/use-locations-view";
import { Checkbox } from "@/shared/components/ui/checkbox";
import { BulkLocationActionBar } from "./bulk-location-action-bar";

export default function LocationsList() {
  const { search, pageSize } = useGetLocationsFilter();
  const {
    departmentIds,
    selectedDepartments,
    setSelectedDepartments,
    reset: resetDepartmentFilter,
    isActive: hasDepartmentFilter,
    isRestoring: isRestoringDepartmentFilter,
  } = useLocationDepartmentFilter();
  const { view, setView, isArchived } = useLocationsView();
  const [createOpen, setCreateOpen] = useState(false)
  const [updateOpen, setUpdateOpen] = useState(false)

  const [selectedLocation, setSelectedLocation] = useState<LocationDto | undefined>(undefined);
  const selectionScope = `${view}:${search ?? ""}:${departmentIds.join(",")}`;
  const [selection, setSelection] = useState<{ scope: string; ids: string[] }>({
    scope: selectionScope,
    ids: [],
  });
 
  const { 
    data, 
    isPending, 
    isError, 
    error, 
    isFetchingNextPage,
    refetch,
    cursorRef
  } = useLocationsList(search, pageSize, !isArchived, departmentIds);

  const selectedIds = selection.scope === selectionScope ? selection.ids : [];
  const visibleIds = data?.items.map((location) => location.id) ?? [];
  const allVisibleSelected = visibleIds.length > 0 && visibleIds.every((id) => selectedIds.includes(id));
  const someVisibleSelected = visibleIds.some((id) => selectedIds.includes(id));

  const setLocationSelected = (id: string, selected: boolean) => {
    setSelection((current) => {
      const ids = current.scope === selectionScope ? current.ids : [];
      return {
        scope: selectionScope,
        ids: selected ? Array.from(new Set([...ids, id])) : ids.filter((value) => value !== id),
      };
    });
  };

  const toggleAllVisible = (selected: boolean) => {
    setSelection({ scope: selectionScope, ids: selected ? visibleIds : [] });
  };

  const clearSelection = () => setSelection({ scope: selectionScope, ids: [] });



  if (isError)
    return (
      <div role="alert" className="rounded-lg border border-red-200 bg-red-50 p-6 text-center">
        <p className="font-medium text-red-800">Не удалось загрузить локации.</p>
        <p className="mt-1 text-sm text-red-700">
        Error: {error instanceof Error ? error.message : "Ошибка"}
        </p>
        <Button className="mt-4" variant="outline" onClick={() => void refetch()}>
          Повторить
        </Button>
      </div>
    );

  return (
    <div>
      <div className="flex justify-between items-center mb-4">
        <h2 className="text-2xl font-semibold">
          Locations ({data?.totalCount})
        </h2>

        {isPending && (
          <span className="text-sm text-gray-500">Updating...</span>
        )}
      </div>

      <div className="mb-4">
        <LocationsFilter />
      </div>

      <div className="mb-4">
        <div className="mb-2 flex items-center justify-between gap-3">
          <h3 className="text-sm font-medium">Filter by departments</h3>
          {hasDepartmentFilter && (
            <Button variant="ghost" size="sm" onClick={resetDepartmentFilter}>
              Reset filter
            </Button>
          )}
        </div>
        <DepartmentsPicker
          selectedDepartments={selectedDepartments}
          onChange={setSelectedDepartments}
          multiselect
        />
        {isRestoringDepartmentFilter && (
          <p className="mt-2 text-sm text-muted-foreground">Restoring department filter...</p>
        )}
      </div>

      {/* 🔹 Фильтр */}
      <div className="mb-6 flex gap-2">
        <button
          onClick={() => setView("active")}
          className={`px-4 py-2 rounded transition-colors ${
            view === "active"
              ? "bg-blue-600 text-white"
              : "bg-gray-200 text-gray-800 hover:bg-gray-300"
          }`}
        >
          Активные
        </button>

        <button
          onClick={() => setView("archived")}
          className={`px-4 py-2 rounded transition-colors ${
            view === "archived"
              ? "bg-blue-600 text-white"
              : "bg-gray-200 text-gray-800 hover:bg-gray-300"
          }`}
        >
          Архивные
        </button>
      </div>

      {visibleIds.length > 0 && (
        <div className="mb-4 flex items-center gap-2 rounded-md border bg-muted/40 px-3 py-2">
          <Checkbox
            id="select-all-visible-locations"
            checked={allVisibleSelected ? true : someVisibleSelected ? "indeterminate" : false}
            onCheckedChange={(checked) => toggleAllVisible(checked === true)}
          />
          <label htmlFor="select-all-visible-locations" className="text-sm font-medium">
            Выбрать все загруженные локации ({visibleIds.length})
          </label>
        </div>
      )}

      
      {/* 🔹 Список */}
      {isPending ? (
        <Spinner />
      ) : !data || data.items.length === 0 ? (
        hasDepartmentFilter ? (
          <div className="rounded-lg border border-dashed p-8 text-center">
            <p className="font-medium">No locations found for the selected departments.</p>
            <Button className="mt-4" variant="outline" onClick={resetDepartmentFilter}>
              Reset filter
            </Button>
          </div>
        ) : (
          <p>{isArchived ? "Архив локаций пуст." : "Локации не найдены."}</p>
        )
      ) : (
        <div className="grid gap-4 grid-cols-1 sm:grid-cols-2 lg:grid-cols-3">
          {data.items.map((location) => (
            <LocationCard
              key={location.id}
              location={location}
              onEdit={() => {
                setSelectedLocation(location)
                setUpdateOpen(true)
              }}
              archived={isArchived}
              selected={selectedIds.includes(location.id)}
              onSelectedChange={(selected) => setLocationSelected(location.id, selected)}
            />
          ))}
        </div>
      )}
    

    {/* кнопка создания */}
    {!isArchived && <div className="mt-6">
      <Button onClick={() => setCreateOpen(true)}>
        Создать локацию
      </Button>
    </div>}

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

    {selectedIds.length > 0 && (
      <BulkLocationActionBar
        selectedIds={selectedIds}
        restoring={isArchived}
        onClear={clearSelection}
      />
    )}
    </div>
  );
}
