import axios from "axios";

const AUTH_API_BASE_URL = process.env.NEXT_PUBLIC_AUTH_SERVICE_URL
  ?? (process.env.NODE_ENV === "production"
    ? "/api/auth"
    : "http://localhost:5200/api/auth");

export type LoginRequest = {
  email: string;
  password: string;
};

type AccessTokenResponse = {
  accessToken: string;
  expiresIn: number;
};

const authClient = axios.create({
  baseURL: AUTH_API_BASE_URL,
  timeout: 10_000,
  withCredentials: true,
  headers: {
    "Content-Type": "application/json",
  },
});

export const authApi = {
  login: async (request: LoginRequest): Promise<AccessTokenResponse> => {
    const response = await authClient.post<AccessTokenResponse>("/login", request);
    return response.data;
  },

  refresh: async (): Promise<AccessTokenResponse> => {
    const response = await authClient.post<AccessTokenResponse>("/refresh");
    return response.data;
  },

  logout: async (): Promise<void> => {
    await authClient.post("/logout");
  },
};
