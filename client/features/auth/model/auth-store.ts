import { create } from "zustand";

type AuthStatus = "initializing" | "authenticated" | "anonymous";

type AuthState = {
  accessToken: string | null;
  roles: string[];
  status: AuthStatus;
  setAccessToken: (accessToken: string) => void;
  clearSession: () => void;
};

export const useAuthStore = create<AuthState>((set) => ({
  accessToken: null,
  roles: [],
  status: "initializing",
  setAccessToken: (accessToken) =>
    set({
      accessToken,
      roles: getRolesFromToken(accessToken),
      status: "authenticated",
    }),
  clearSession: () =>
    set({ accessToken: null, roles: [], status: "anonymous" }),
}));

const roleClaim = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role";

function getRolesFromToken(accessToken: string): string[] {
  try {
    const payloadPart = accessToken.split(".")[1];
    if (!payloadPart) return [];

    const base64 = payloadPart
      .replace(/-/g, "+")
      .replace(/_/g, "/")
      .padEnd(Math.ceil(payloadPart.length / 4) * 4, "=");
    const payload = JSON.parse(atob(base64)) as Record<string, unknown>;
    const value = payload.role ?? payload.roles ?? payload[roleClaim];
    const roles = Array.isArray(value) ? value : value ? [value] : [];

    return Array.from(
      new Set(roles.filter((role): role is string => typeof role === "string")),
    );
  } catch {
    return [];
  }
}

export const getAccessToken = () => useAuthStore.getState().accessToken;

export const getAuthStatus = () => useAuthStore.getState().status;

export const setAccessToken = (accessToken: string) =>
  useAuthStore.getState().setAccessToken(accessToken);

export const clearSession = () =>
  useAuthStore.getState().clearSession();
