import { create } from "zustand";
import type { DepartmentWithChildrenDto } from "@/entities/departments/types";

type TreeStore = {
  expandedIds: string[];
  selectedId: string | null;
  childrenByParentId: Record<string, DepartmentWithChildrenDto[]>;
  toggleExpanded: (id: string) => void;
  select: (id: string | null) => void;
  setChildren: (parentId: string, children: DepartmentWithChildrenDto[]) => void;
  resetChildren: () => void;
  updateActivity: (departmentId: string, isActive: boolean) => void;
};

export const useTreeStore = create<TreeStore>((set) => ({
  expandedIds: [],
  selectedId: null,
  childrenByParentId: {},
  toggleExpanded: (id) =>
    set((state) => ({
      expandedIds: state.expandedIds.includes(id)
        ? state.expandedIds.filter((expandedId) => expandedId !== id)
        : [...state.expandedIds, id],
    })),
  select: (id) => set({ selectedId: id }),
  setChildren: (parentId, children) =>
    set((state) => ({
      childrenByParentId: {
        ...state.childrenByParentId,
        [parentId]: children,
      },
    })),
  resetChildren: () => set({ childrenByParentId: {} }),
  updateActivity: (departmentId, isActive) =>
    set((state) => ({
      childrenByParentId: Object.fromEntries(
        Object.entries(state.childrenByParentId).map(([parentId, children]) => [
          parentId,
          children.map((child) =>
            child.id === departmentId ? { ...child, isActive } : child
          ),
        ])
      ),
    })),
}));
