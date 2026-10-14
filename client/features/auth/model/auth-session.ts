import { getSession, signIn, signOut } from "next-auth/react";
import { authApi, type LoginRequest } from "../api/auth-api";
import { clearSession, setSession } from "./auth-store";

let sessionPromise: Promise<string | null> | null = null;

export function getCurrentAccessToken(): Promise<string | null> {
  if (sessionPromise) return sessionPromise;

  sessionPromise = getSession()
    .then((session) => {
      if (!session?.accessToken) {
        clearSession();
        return null;
      }

      setSession(session.accessToken, session.roles, session.user?.name ?? null);
      return session.accessToken;
    })
    .finally(() => {
      sessionPromise = null;
    });

  return sessionPromise;
}

export async function loginSession(
  request: LoginRequest,
  returnUrl?: string | null,
): Promise<void> {
  await authApi.login(request);

  if (returnUrl) {
    try {
      const target = new URL(returnUrl, window.location.origin);
      if (
        target.origin === window.location.origin &&
        target.pathname === "/connect/authorize"
      ) {
        window.location.assign(target.href);
        return;
      }
    } catch {
      // Ignore malformed or untrusted return URLs and start a fresh OIDC flow.
    }
  }

  await signIn("directory", { callbackUrl: "/" });
}

export async function continueSession(): Promise<void> {
  await signIn("directory", { callbackUrl: "/" });
}

export async function logoutSession(): Promise<void> {
  clearSession();
  await signOut({ callbackUrl: "/login" });
}
