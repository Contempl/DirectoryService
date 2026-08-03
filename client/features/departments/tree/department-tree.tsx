"use client"

import { TreeProvider, TreeView } from "@/shared/components/kibo-ui/tree";
import { useDepartmentRoots } from "./model/use-department-roots";
import { DepartmentTreeNode } from "./department-tree-node";
import { Spinner } from "@/shared/components/ui/spinner";
import { Button } from "@/shared/components/ui/button";
import { useTreeStore } from "../model/use-tree-store";

export function DepartmentTree() {
  const expandedIds = useTreeStore((state) => state.expandedIds);
  const selectedId = useTreeStore((state) => state.selectedId);
  const toggleExpanded = useTreeStore((state) => state.toggleExpanded);
  const select = useTreeStore((state) => state.select);

  const {
    data,
    isPending,
    isError,
    error,
    hasNextPage,
    isFetchingNextPage,
    fetchNextPage,
  } = useDepartmentRoots(20);

  if (isPending) return <Spinner />;

  if (isError)
    return <p className="text-red-600">{error instanceof Error ? error.message : "Ошибка"}</p>;

  if (!data || data.length === 0)
    return <p className="text-muted-foreground">Нет подразделений</p>;

  return (
    <div className="flex flex-col gap-2">
      <TreeProvider
        showLines
        expandedIds={expandedIds}
        onExpandedChange={toggleExpanded}
        selectedIds={selectedId ? [selectedId] : []}
        onSelectionChange={(ids) => select(ids.at(-1) ?? null)}
      >
        <TreeView>
          {data.map((department, index) => (
            <DepartmentTreeNode
              key={department.id}
              department={department}
              level={0}
              isLast={index === data.length - 1}
            />
          ))}
        </TreeView>
      </TreeProvider>

      {hasNextPage && <Button
        variant="ghost"
        size="sm"
        className="self-start"
        disabled={isFetchingNextPage}
        onClick={() => void fetchNextPage()}
      >
        Показать ещё
      </Button>}
    </div>
  );
}
