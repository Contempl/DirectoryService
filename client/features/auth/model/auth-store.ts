import { create } from "zustand";

type AuthStatus = "initializing" | "authenticated" | "anonymous";

type AuthState = {
  accessToken: string | null;
  status: AuthStatus;
  setAccessToken: (accessToken: string) => void;
  clearSession: () => void;
};

export const useAuthStore = create<AuthState>((set) => ({
  accessToken: null,
  status: "initializing",
  setAccessToken: (accessToken) =>
    set({ accessToken, status: "authenticated" }),
  clearSession: () =>
    set({ accessToken: null, status: "anonymous" }),
}));

export const getAccessToken = () => useAuthStore.getState().accessToken;

export const getAuthStatus = () => useAuthStore.getState().status;

export const setAccessToken = (accessToken: string) =>
  useAuthStore.getState().setAccessToken(accessToken);

export const clearSession = () =>
  useAuthStore.getState().clearSession();
