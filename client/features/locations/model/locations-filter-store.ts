import { create } from "zustand";
import { useShallow } from "zustand/react/shallow";

export type LocationsFilterState = {
    search?: string;
    pageSize: number;
    isActive: boolean,
};

type Actions = {
    setSearch: (input: LocationsFilterState["search"]) => void;
    setIsActive: (isActive: LocationsFilterState["isActive"]) => void;
}

type LocationsFilterStore = LocationsFilterState & Actions;


const initialState: LocationsFilterState = {
    search: "",
    pageSize: 5,
    isActive: true,
}

const useLocationsFilterStore = create<LocationsFilterStore>((set => ({
    ...initialState,
    setSearch: (input: LocationsFilterState["search"]) => 
      set(() => ({search: input?.trim() || undefined})),
    setIsActive: (value: boolean) => set(() => ({ isActive: value })),
})));


export const useGetLocationsFilter = () => {
  return useLocationsFilterStore(
    useShallow((state) => ({
      search: state.search,
      pageSize: state.pageSize,
      isActive: state.isActive,
    }))
  );
};

export const setFilterSearch = (input: LocationsFilterState["search"]) => 
    useLocationsFilterStore.getState().setSearch(input || "");

export const setFilterIsActive = (value: boolean) =>
  useLocationsFilterStore.getState().setIsActive(value);
