import { authApi, type LoginRequest } from "../api/auth-api";
import { clearSession, setAccessToken } from "./auth-store";

let refreshPromise: Promise<string> | null = null;

export async function loginSession(request: LoginRequest): Promise<void> {
  const { accessToken } = await authApi.login(request);
  setAccessToken(accessToken);
}

export function refreshAccessToken(): Promise<string> {
  if (refreshPromise) return refreshPromise;

  refreshPromise = authApi.refresh()
    .then(({ accessToken }) => {
      setAccessToken(accessToken);
      return accessToken;
    })
    .catch((error: unknown) => {
      clearSession();
      throw error;
    })
    .finally(() => {
      refreshPromise = null;
    });

  return refreshPromise;
}

export async function logoutSession(): Promise<void> {
  try {
    await authApi.logout();
  } finally {
    clearSession();
  }
}
