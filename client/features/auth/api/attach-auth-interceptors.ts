import axios, {
  type AxiosInstance,
  type InternalAxiosRequestConfig,
} from "axios";
import { getAccessToken, getAuthStatus } from "../model/auth-store";
import { refreshAccessToken } from "../model/auth-session";

type RetryableRequestConfig = InternalAxiosRequestConfig & {
  _authRetry?: boolean;
};

export function attachAuthInterceptors(client: AxiosInstance) {
  client.interceptors.request.use((config) => {
    const accessToken = getAccessToken();

    if (accessToken) {
      config.headers.Authorization = `Bearer ${accessToken}`;
    }

    return config;
  });

  client.interceptors.response.use(
    (response) => response,
    async (error: unknown) => {
      if (!axios.isAxiosError(error) || error.response?.status !== 401) {
        return Promise.reject(error);
      }

      if (getAuthStatus() === "anonymous") {
        return Promise.reject(error);
      }

      const request = error.config as RetryableRequestConfig | undefined;

      if (!request || request._authRetry) {
        return Promise.reject(error);
      }

      request._authRetry = true;

      try {
        const accessToken = await refreshAccessToken();
        request.headers.Authorization = `Bearer ${accessToken}`;
        return client(request);
      } catch {
        return Promise.reject(error);
      }
    },
  );
}
