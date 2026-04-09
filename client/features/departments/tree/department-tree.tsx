"use client"

import { TreeProvider, TreeView } from "@/shared/components/kibo-ui/tree";
import { useDepartmentRoots } from "./model/use-department-roots";
import { DepartmentTreeNode } from "./department-tree-node";
import { Spinner } from "@/shared/components/ui/spinner";
import { Button } from "@/shared/components/ui/button";
import { useState } from "react";

export function DepartmentTree() {
  const [page, setPage] = useState(1);
  const { data, isPending, isError, error } = useDepartmentRoots(page, 20, 3);

  if (isPending) return <Spinner />;

  if (isError)
    return <p className="text-red-600">{error instanceof Error ? error.message : "Ошибка"}</p>;

  if (!data || data.length === 0)
    return <p className="text-muted-foreground">Нет подразделений</p>;

  return (
    <div className="flex flex-col gap-2">
      <TreeProvider showLines>
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

      <Button
        variant="ghost"
        size="sm"
        className="self-start"
        onClick={() => setPage((p) => p + 1)}
      >
        Показать ещё
      </Button>
    </div>
  );
}