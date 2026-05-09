import { create } from "zustand";

export type DepartmentListId = string;

const DEFAULT_STATE_ID = "__default__";

export interface DepartmentListState {
  search: string;
  pageSize: number;
}

type DepartmentListStates = Record<DepartmentListId, DepartmentListState | undefined>;

const initialState: DepartmentListState = {
  search: "",
  pageSize: 20,
};

const resolveStateId = (stateId?: DepartmentListId) => stateId ?? DEFAULT_STATE_ID;

const getOrCreate = (
  states: DepartmentListStates,
  stateId?: DepartmentListId
): DepartmentListState => {
  const id = resolveStateId(stateId);
  return states[id] ?? { ...initialState };
};

const useDepartmentListStore = create<DepartmentListStates>()(() => ({}));

// --- селекторы ---
export const useDepartmentSearch = (stateId?: DepartmentListId) =>
  useDepartmentListStore((states) => getOrCreate(states, stateId).search);

export const useDepartmentPageSize = (stateId?: DepartmentListId) =>
  useDepartmentListStore((states) => getOrCreate(states, stateId).pageSize);

// --- сеттеры ---
export const setDepartmentSearch = (search: string, stateId?: DepartmentListId) =>
  useDepartmentListStore.setState((states) => ({
    [resolveStateId(stateId)]: {
      ...getOrCreate(states, stateId),
      search,
    },
  }));