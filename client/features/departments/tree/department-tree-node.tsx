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
import { Folder, FileText } from "lucide-react";
import { useTreeStore } from "../model/use-tree-store";

type DepartmentTreeNodeProps = {
  department: DepartmentWithChildrenDto;
  level?: number;
  isLast?: boolean;
  parentPath?: boolean[];
};

export function DepartmentTreeNode({
  department,
  level = 0,
  isLast = false,
  parentPath = [],
}: DepartmentTreeNodeProps) {
  const nodeState = useTreeStore((state) => state.nodes[department.id]);
  const toggle = useTreeStore((state) => state.toggle);
  const loadMore = useTreeStore((state) => state.loadMore);
  
  const isNodeExpanded = nodeState?.isExpanded ?? false;
  const page = nodeState?.page ?? 1;

  // --- НАЧАЛО БЛОКА ОТЛАДКИ ---
  console.log(`[${department.name}] hasMoreChildren с бэкенда:`, department.hasMoreChildren);
  console.log(`[${department.name}] Раскрыт ли узел (isNodeExpanded):`, isNodeExpanded);
  
  const shouldFetch = isNodeExpanded && department.hasMoreChildren;
  console.log(`[${department.name}] РЕШЕНИЕ: Запускать ли запрос (shouldFetch):`, shouldFetch);
  // --- КОНЕЦ БЛОКА ОТЛАДКИ ---

  const { data: lazyChildren, isFetching } = useDepartmentChildren(
    department.id,
    page,
    20,
    shouldFetch // Используем нашу отладочную переменную
  );
  
  //... остальной код компонента без изменений
  const hasChildren = department.children.length > 0 || department.hasMoreChildren;

  const allChildren = [
    ...department.children,
    ...(lazyChildren ?? []),
  ].filter(
    (child, index, self) => self.findIndex((c) => c.id === child.id) === index
  );

  const isExhausted = lazyChildren !== undefined && lazyChildren.length < 20;
  const showLoadMore = department.hasMoreChildren && !isExhausted;

  return (
    <TreeNode
      nodeId={department.id}
      level={level}
      isLast={isLast}
      parentPath={parentPath}
    >
      <TreeNodeTrigger onClick={() => toggle(department.id)}>
        <TreeExpander hasChildren={hasChildren} />
        
        {hasChildren ? (
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
      </TreeNodeTrigger>

      <TreeNodeContent hasChildren={hasChildren}>
        {allChildren.map((child, index) => (
          <DepartmentTreeNode
            key={child.id}
            department={child}
            level={level + 1}
            isLast={index === allChildren.length - 1}
            parentPath={[...parentPath, isLast]}
          />
        ))}
        
        {isFetching && (
          <div className="flex items-center ml-6 my-2 text-xs text-muted-foreground">
            <Spinner className="mr-2 h-3 w-3" /> Загрузка...
          </div>
        )}
        
        {showLoadMore && !isFetching && (
          <Button
            variant="link"
            size="sm"
            className="text-xs text-blue-600 ml-4 h-6"
            onClick={(e) => {
              e.stopPropagation();
              loadMore(department.id) ;
            }}
          >
            Показать ещё...
          </Button>
        )}
      </TreeNodeContent>
    </TreeNode>
  );
}