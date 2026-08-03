"use client";

import { useTreeStore } from "@/features/departments/model/use-tree-store";
import { usePositionsList } from "@/features/positions/models/use-positions-list";
import { Spinner } from "@/shared/components/ui/spinner";

export function DepartmentPositionsList() {
  const selectedId = useTreeStore((state) => state.selectedId);
  const { data, isPending, isError, error, isFetchingNextPage, cursorRef } =
    usePositionsList(
      undefined,
      20,
      true,
      selectedId ? [selectedId] : [],
      selectedId !== null
    );

  if (!selectedId) {
    return (
      <p className="text-sm text-muted-foreground">
        Выберите подразделение в дереве слева.
      </p>
    );
  }

  if (isError) {
    return (
      <p className="text-sm text-red-600">
        {error instanceof Error ? error.message : "Не удалось загрузить позиции"}
      </p>
    );
  }

  if (isPending) {
    return <Spinner />;
  }

  if (!data || data.items.length === 0) {
    return <p className="text-sm text-muted-foreground">У подразделения нет позиций.</p>;
  }

  return (
    <div className="flex flex-col gap-3">
      <p className="text-sm text-muted-foreground">
        Найдено: {data.totalCount}
      </p>

      {data.items.map((position) => (
        <article className="rounded-lg border p-4" key={position.id}>
          <div className="flex items-center justify-between gap-3">
            <h3 className="font-medium">{position.name}</h3>
            <span className="text-xs text-muted-foreground">
              {position.isActive ? "Активна" : "Неактивна"}
            </span>
          </div>
          {position.description && (
            <p className="mt-2 text-sm text-muted-foreground">
              {position.description}
            </p>
          )}
        </article>
      ))}

      <div className="flex justify-center py-2" ref={cursorRef}>
        {isFetchingNextPage && <Spinner />}
      </div>
    </div>
  );
}
