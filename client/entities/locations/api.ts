import { apiClient } from "@/shared/api/axios-instance";
import { LocationDto } from "./types";
import { PagedResult } from "@/shared/types";


export type GetLocationsRequest = {
    department_ids?: string[], 
    search?: string,
    is_active: boolean,
    page: number,
    page_size: number
}

export type CreateLocationDto = {
  name: string;
  city: string;
  street: string;
  house: string;
  apartment?: string | null;
  timezone: string;
}

export type CreateLocationRequest ={
  name: string;
  city: string;
  street: string;
  house: string;
  apartment?: string | null;
  timezone: string;
}

export const locationsApi = {
    getLocations: async (query?: GetLocationsRequest): Promise<PagedResult<LocationDto>> => {
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