import { create } from "zustand";
import { useShallow } from "zustand/react/shallow";
import { DepartmentShortDto } from "@/entities/departments/types";

export type PositionsFilterState = {
  search?: string;
  pageSize: number;
  isActive: boolean;
  selectedDepartments: DepartmentShortDto[];
};

type Actions = {
  setSearch: (input: PositionsFilterState["search"]) => void;
  setIsActive: (isActive: PositionsFilterState["isActive"]) => void;
  setSelectedDepartments: (departments: DepartmentShortDto[]) => void;
};

type PositionsFilterStore = PositionsFilterState & Actions;

const initialState: PositionsFilterState = {
  search: "",
  pageSize: 10,
  isActive: true,
  selectedDepartments: [],
};

const usePositionsFilterStore = create<PositionsFilterStore>((set) => ({
  ...initialState,
  setSearch: (input) => set(() => ({ search: input?.trim() || undefined })),
  setIsActive: (value) => set(() => ({ isActive: value })),
  setSelectedDepartments: (departments) => set(() => ({ selectedDepartments: departments })),
}));

export const useGetPositionsFilter = () => {
  return usePositionsFilterStore(
    useShallow((state) => ({
      search: state.search,
      pageSize: state.pageSize,
      isActive: state.isActive,
      selectedDepartments: state.selectedDepartments,
    }))
  );
};

export const setFilterSearch = (input: PositionsFilterState["search"]) =>
  usePositionsFilterStore.getState().setSearch(input || "");

export const setFilterIsActive = (value: boolean) =>
  usePositionsFilterStore.getState().setIsActive(value);

export const setFilterSelectedDepartments = (departments: DepartmentShortDto[]) =>
  usePositionsFilterStore.getState().setSelectedDepartments(departments);