"use client"

import { DepartmentShortDto } from "@/entities/departments/types";
import { Spinner } from "@/shared/components/ui/spinner";
import { DepartmentCard } from "./department-card";
import { DepartmentListId, useDepartmentPageSize, useDepartmentSearch } from "./model/department-list-store";
import { useDepartmentsList } from "./model/use-department-list";

type DepartmentsListProps = {
  selectedDepartments: DepartmentShortDto[];
  onToggle: (department: DepartmentShortDto) => void;
  excludeIds?: string[];
  stateId?: DepartmentListId;
};

export function DepartmentsList({ selectedDepartments, onToggle, excludeIds, stateId }: DepartmentsListProps) {
  const search = useDepartmentSearch(stateId);
  const pageSize = useDepartmentPageSize(stateId);

  const { data, isPending, isError, error, isFetchingNextPage, cursorRef } =
    useDepartmentsList(search, pageSize, stateId);

  if (isPending) return <Spinner />;

  if (isError)
    return (
      <p className="text-red-600">
        {error instanceof Error ? error.message : "Ошибка"}
      </p>
    );

  const items = data?.items.filter((d) => !excludeIds?.includes(d.id)) ?? [];

  if (items.length === 0) return <p className="text-muted-foreground text-sm">Ничего не найдено</p>;

  return (
    <div className="flex flex-col gap-2">
      {items.map((department) => (
        <DepartmentCard
          key={department.id}
          department={department}
          isSelected={selectedDepartments.some((s) => s.id === department.id)}
          onToggle={onToggle}
        />
      ))}

      <div ref={cursorRef} className="flex justify-center py-2">
        {isFetchingNextPage && <Spinner />}
      </div>
    </div>
  );
}