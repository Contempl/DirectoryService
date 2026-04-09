import { create } from "zustand";
import { persist, createJSONStorage } from "zustand/middleware";

type NodeState = {
  isExpanded: boolean;
  page: number;
};

type TreeStore = {
  nodes: Record<string, NodeState>;
  toggle: (id: string) => void;
  loadMore: (id: string) => void;
};

export const useTreeStore = create<TreeStore>()(
  persist(
    (set) => ({
      nodes: {},
      toggle: (id) =>
        set((state) => {
          const node = state.nodes[id];
          return {
            nodes: {
              ...state.nodes,
              [id]: {
                isExpanded: !node?.isExpanded,
                page: node?.page ?? 1,
              },
            },
          };
        }),
      loadMore: (id) => {
      console.log("loadMore вызван для:", id);
        set((state) => {
          const node = state.nodes[id];
          return {
            nodes: {
              ...state.nodes,
              [id]: {
                ...node,
                isExpanded: true,
                page: (node?.page ?? 1) + 1,
              },
            },
          };
        })
      },
    }),
    {
      name: "departments-tree-state",
      storage: createJSONStorage(() => sessionStorage), // сохраняем в сессию
    }
  )
);