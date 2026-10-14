import { create } from "zustand";

type AuthStatus = "initializing" | "authenticated" | "anonymous";

type AuthState = {
  accessToken: string | null;
  roles: string[];
  name: string | null;
  status: AuthStatus;
  setSession: (accessToken: string, roles: string[], name: string | null) => void;
  clearSession: () => void;
};

export const useAuthStore = create<AuthState>((set) => ({
  accessToken: null,
  roles: [],
  name: null,
  status: "initializing",
  setSession: (accessToken, roles, name) =>
    set({
      accessToken,
      roles,
      name,
      status: "authenticated",
    }),
  clearSession: () =>
    set({ accessToken: null, roles: [], name: null, status: "anonymous" }),
}));

export const getAccessToken = () => useAuthStore.getState().accessToken;

export const getAuthStatus = () => useAuthStore.getState().status;

export const setSession = (accessToken: string, roles: string[], name: string | null) =>
  useAuthStore.getState().setSession(accessToken, roles, name);

export const clearSession = () =>
  useAuthStore.getState().clearSession();
