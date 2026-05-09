import { DepartmentShortDto } from "@/entities/departments/types";
import { create } from "zustand";
import { useShallow } from "zustand/react/shallow";

export type LocationsFilterState = {
    search?: string;
    pageSize: number;
    isActive: boolean,
    selectedDepartments: DepartmentShortDto[];
};

type Actions = {
    setSearch: (input: LocationsFilterState["search"]) => void;
    setIsActive: (isActive: LocationsFilterState["isActive"]) => void;
    setSelectedDepartments: (departments: DepartmentShortDto[]) => void;
}

type LocationsFilterStore = LocationsFilterState & Actions;


const initialState: LocationsFilterState = {
    search: "",
    pageSize: 5,
    isActive: true,
    selectedDepartments: [],
}

const useLocationsFilterStore = create<LocationsFilterStore>((set => ({
    ...initialState,
    setSearch: (input: LocationsFilterState["search"]) => 
      set(() => ({search: input?.trim() || undefined})),
    setIsActive: (value: boolean) => set(() => ({ isActive: value })),
    setSelectedDepartments: (departments) => set(() => ({ selectedDepartments: departments })),
})));


export const useGetLocationsFilter = () => {
  return useLocationsFilterStore(
    useShallow((state) => ({
      search: state.search,
      pageSize: state.pageSize,
      isActive: state.isActive,
      selectedDepartments: state.selectedDepartments,
    }))
  );
};

export const setFilterSearch = (input: LocationsFilterState["search"]) => 
    useLocationsFilterStore.getState().setSearch(input || "");

export const setFilterIsActive = (value: boolean) =>
  useLocationsFilterStore.getState().setIsActive(value);

export const setFilterSelectedDepartments = (departments: DepartmentShortDto[]) =>
  useLocationsFilterStore.getState().setSelectedDepartments(departments);