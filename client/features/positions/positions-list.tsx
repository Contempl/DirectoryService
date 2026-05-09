"use client"

import { useState } from "react";
import { PositionDto } from "@/entities/positions/types";
import { PositionCard } from "./position-card";
import { DepartmentsPicker } from "@/features/departments/departments-picker";
import { CreatePositionDialog } from "./create-position-dialog";
import { EditPositionDialog } from "./edit-position-dialog";
import { Button } from "@/shared/components/ui/button";
import { Spinner } from "@/shared/components/ui/spinner";
import { setFilterIsActive, 
    setFilterSelectedDepartments, 
    useGetPositionsFilter } from "./models/positions-filter-store";
import { useDeletePosition } from "./models/use-delete-position";
import { usePositionsList } from "./models/use-positions-list";
import { PositionsFilter } from "./positions-filter";

export default function PositionsList() {
  const { search, pageSize, isActive, selectedDepartments } = useGetPositionsFilter();

  const [createOpen, setCreateOpen] = useState(false);
  const [updateOpen, setUpdateOpen] = useState(false);
  const [selectedPosition, setSelectedPosition] = useState<PositionDto | undefined>(undefined);

  const { deletePosition } = useDeletePosition();

  const { data, isPending, isError, error, isFetchingNextPage, cursorRef } =
    usePositionsList(
      search,
      pageSize,
      isActive,
      selectedDepartments.map((d) => d.id)
    );

  if (isError)
    return (
      <p className="text-red-600">
        {error instanceof Error ? error.message : "Ошибка"}
      </p>
    );

  return (
    <div>
      <div className="flex justify-between items-center mb-4">
        <h2 className="text-2xl font-semibold">
          Positions ({data?.totalCount})
        </h2>
        {isPending && <span className="text-sm text-gray-500">Загрузка...</span>}
      </div>

      <div className="mb-4">
        <PositionsFilter />
      </div>

      <div className="mb-4">
        <DepartmentsPicker
          selectedDepartments={selectedDepartments}
          onChange={setFilterSelectedDepartments}
          multiselect
        />
      </div>

      <div className="mb-6 flex gap-2">
        <button
          onClick={() => setFilterIsActive(true)}
          className={`px-4 py-2 rounded transition-colors ${
            isActive
              ? "bg-blue-600 text-white"
              : "bg-gray-200 text-gray-800 hover:bg-gray-300"
          }`}
        >
          Active
        </button>
        <button
          onClick={() => setFilterIsActive(false)}
          className={`px-4 py-2 rounded transition-colors ${
            !isActive
              ? "bg-blue-600 text-white"
              : "bg-gray-200 text-gray-800 hover:bg-gray-300"
          }`}
        >
          Inactive
        </button>
      </div>

      {!data || data.items.length === 0 ? (
        <p>No positions</p>
      ) : (
        <div className="grid gap-4 grid-cols-1 sm:grid-cols-2 lg:grid-cols-3">
          {isPending ? (
            <Spinner />
          ) : (
            data.items.map((position) => (
              <PositionCard
                key={position.id}
                position={position}
                onEdit={() => {
                  setSelectedPosition(position);
                  setUpdateOpen(true);
                }}
                onDelete={() => deletePosition(position.id)}
              />
            ))
          )}
        </div>
      )}

      <div className="mt-6">
        <Button onClick={() => setCreateOpen(true)}>
          Создать позицию
        </Button>
      </div>

      <CreatePositionDialog
        open={createOpen}
        onOpenChange={setCreateOpen}
      />

      {selectedPosition && (
        <EditPositionDialog
          key={selectedPosition.id}
          position={selectedPosition}
          open={selectedPosition !== undefined && updateOpen}
          onOpenChange={setUpdateOpen}
        />
      )}

      <div ref={cursorRef} className="flex justify-center py-4">
        {isFetchingNextPage && <Spinner />}
      </div>
    </div>
  );
}