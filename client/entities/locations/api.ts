import { apiClient } from "@/shared/api/axios-instance";
import { LocationDto, PagedResult } from "./types";


export type GetLocationsQuery = {
    departmentIds?: string[], 
    search?: string,
    isActive: boolean,
    page: number,
    pageSize: number
}

export type CreateLocationDto = {
  Name: string;
  City: string;
  Street: string;
  House: string;
  Apartment?: string | null;
  Timezone: string;
}

export type CreateLocationRequest ={
  CreateLocationDto: CreateLocationDto;
}

export const locationsApi = {
    getLocations: async (query?: GetLocationsQuery): Promise<PagedResult<LocationDto>> => {
        const response = await apiClient.get<PagedResult<LocationDto>>("/locations", {
            params: query
    });

        return response.data;
    },

    createLocation: async (request: CreateLocationRequest) => {
        const response = await apiClient.post("/locations", request);
        
        return response.data;
    },
}