import axios from "axios";
import { attachAuthInterceptors } from "@/features/auth/api/attach-auth-interceptors";
import { apiClient } from "@/shared/api/axios-instance";
import type { Envelope } from "@/shared/api/envelope";

const AUTH_SERVICE_BASE_URL = process.env.NEXT_PUBLIC_AUTH_SERVICE_API_URL
  ?? (process.env.NODE_ENV === "production"
    ? "/api"
    : "http://localhost:5200/api");

export const systemRoles = ["User", "Employee", "Moderator", "Admin"] as const;

export type SystemRole = typeof systemRoles[number];

export type UserDto = {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  isActive: boolean;
  roles: string[];
};

export type PagedUsersResult = {
  items: UserDto[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
};

const usersClient = axios.create({
  baseURL: AUTH_SERVICE_BASE_URL,
  timeout: 10_000,
  headers: { "Content-Type": "application/json" },
});

attachAuthInterceptors(usersClient);

export const usersApi = {
  getUsers: async (page: number, pageSize: number): Promise<PagedUsersResult> => {
    const response = await apiClient.get<Envelope<PagedUsersResult>>(
      "/auth-integration/users",
      { params: { page, pageSize } },
    );

    if (!response.data.result) {
      throw new Error("Users response did not contain a result.");
    }

    return response.data.result;
  },

  assignRole: async (userId: string, role: SystemRole): Promise<void> => {
    await usersClient.post(`/users/${userId}/roles`, { role });
  },

  removeRole: async (userId: string, role: string): Promise<void> => {
    await usersClient.delete(`/users/${userId}/roles/${encodeURIComponent(role)}`);
  },
};

export function isUsersAccessDenied(error: unknown): boolean {
  return axios.isAxiosError(error) && error.response?.status === 403;
}
