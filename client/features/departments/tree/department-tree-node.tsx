"use client"

import { DepartmentWithChildrenDto } from "@/entities/departments/types";
import {
  TreeNode,
  TreeNodeTrigger,
  TreeNodeContent,
  TreeExpander,
  TreeLabel,
} from "@/shared/components/kibo-ui/tree";
import { Tooltip, TooltipContent, TooltipProvider, TooltipTrigger } from "@/shared/components/ui/tooltip";
import { useDepartmentChildren } from "./model/use-department-children";
import { Button } from "@/shared/components/ui/button";
import { Spinner } from "@/shared/components/ui/spinner";
import { Folder, FileText, Move } from "lucide-react";
import { useTreeStore } from "../model/use-tree-store";
import { useEffect, useState } from "react";
import { MoveDepartmentDialog } from "../move-department-dialog";

type DepartmentTreeNodeProps = {
  department: DepartmentWithChildrenDto;
  level?: number;
  isLast?: boolean;
  parentPath?: boolean[];
  currentParentName?: string | null;
};

export function DepartmentTreeNode({
  department,
  level = 0,
  isLast = false,
  parentPath = [],
  currentParentName = null,
}: DepartmentTreeNodeProps) {
  const [moveDialogOpen, setMoveDialogOpen] = useState(false);
  const isNodeExpanded = useTreeStore((state) =>
    state.expandedIds.includes(department.id)
  );
  const storedChildren = useTreeStore(
    (state) => state.childrenByParentId[department.id]
  );
  const setChildren = useTreeStore((state) => state.setChildren);

  const shouldFetch =
    isNodeExpanded && department.hasChildren && storedChildren === undefined;

  const {
    data: lazyChildren,
    isFetching,
    isFetchingNextPage,
    hasNextPage,
    fetchNextPage,
  } = useDepartmentChildren(
    department.id,
    20,
    shouldFetch
  );

  useEffect(() => {
    if (lazyChildren !== undefined) {
      setChildren(department.id, lazyChildren);
    }
  }, [department.id, lazyChildren, setChildren]);

  const allChildren = storedChildren ?? lazyChildren ?? department.children;

  const showLoadMore = department.hasChildren && hasNextPage;

  return (
    <TreeNode
      nodeId={department.id}
      level={level}
      isLast={isLast}
      parentPath={parentPath}
    >
      <TreeNodeTrigger>
        <TreeExpander hasChildren={department.hasChildren} />
        
        {department.hasChildren ? (
          <Folder className="h-4 w-4 text-blue-500 mr-2 shrink-0" />
        ) : (
          <FileText className="h-4 w-4 text-gray-400 mr-2 shrink-0" />
        )}

        <TooltipProvider>
          <Tooltip delayDuration={300}>
            <TooltipTrigger asChild>
              <TreeLabel className={!department.isActive ? "opacity-50" : ""}>
                {department.name}
                <span className="text-xs text-muted-foreground ml-2">
                  {department.identifier}
                </span>
              </TreeLabel>
            </TooltipTrigger>
            <TooltipContent side="right">
              <p>Путь: <span className="font-mono text-xs">{department.path}</span></p>
              <p>Уровень: {department.depth}</p>
            </TooltipContent>
          </Tooltip>
        </TooltipProvider>

        {!department.isActive && (
          <span className="text-[10px] px-1.5 py-0.5 rounded border bg-gray-100 text-gray-500 ml-2">
            Архив
          </span>
        )}
        {department.isActive && (
          <Button
            type="button"
            variant="ghost"
            size="sm"
            className="ml-auto h-7 opacity-0 group-hover:opacity-100 focus:opacity-100"
            onClick={(event) => {
              event.stopPropagation();
              setMoveDialogOpen(true);
            }}
          >
            <Move className="h-3.5 w-3.5" />
            Перенести
          </Button>
        )}
      </TreeNodeTrigger>

      <MoveDepartmentDialog
        department={department}
        currentParentName={currentParentName}
        open={moveDialogOpen}
        onOpenChange={setMoveDialogOpen}
      />

      <TreeNodeContent hasChildren={department.hasChildren}>
        {allChildren.map((child, index) => (
          <DepartmentTreeNode
            key={child.id}
            department={child}
            level={level + 1}
            isLast={index === allChildren.length - 1}
            parentPath={[...parentPath, isLast]}
            currentParentName={department.name}
          />
        ))}
        
        {isFetching && (
          <div className="flex items-center ml-6 my-2 text-xs text-muted-foreground">
            <Spinner className="mr-2 h-3 w-3" /> Загрузка...
          </div>
        )}
        
        {showLoadMore && !isFetchingNextPage && (
          <Button
            variant="link"
            size="sm"
            className="text-xs text-blue-600 ml-4 h-6"
            onClick={(e) => {
              e.stopPropagation();
              void fetchNextPage();
            }}
          >
            Показать ещё...
          </Button>
        )}
      </TreeNodeContent>
    </TreeNode>
  );
}
