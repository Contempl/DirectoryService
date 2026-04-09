// model/use-expanded-nodes.ts
import { useState, useEffect } from "react";

export type NodeState = {
  isExpanded: boolean;
  page: number;
  hasMore: boolean;
};

const STORAGE_KEY = "departments-tree-state";

export function useExpandedNodes() {
  const [nodes, setNodes] = useState<Record<string, NodeState>>(() => {
    // Восстанавливаем из sessionStorage при загрузке
    if (typeof window !== "undefined") {
      const saved = sessionStorage.getItem(STORAGE_KEY);
      if (saved) return JSON.parse(saved);
    }
    return {};
  });

  // Сохраняем при каждом изменении
  useEffect(() => {
    sessionStorage.setItem(STORAGE_KEY, JSON.stringify(nodes));
  }, [nodes]);

  const toggle = (id: string, hasMoreChildren: boolean) => {
    setNodes((prev) => ({
      ...prev,
      [id]: {
        isExpanded: !prev[id]?.isExpanded,
        page: prev[id]?.page ?? 1,
        hasMore: hasMoreChildren,
      },
    }));
  };

  const loadMore = (id: string) => {
    setNodes((prev) => ({
      ...prev,
      [id]: {
        ...prev[id],
        page: (prev[id]?.page ?? 1) + 1,
      },
    }));
  };

  const isExpanded = (id: string) => nodes[id]?.isExpanded ?? false;
  const getPage = (id: string) => nodes[id]?.page ?? 1;

  return { toggle, loadMore, isExpanded, getPage };
}