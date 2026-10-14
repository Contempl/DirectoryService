"use client";

import { useEffect } from "react";
import { usePathname, useRouter } from "next/navigation";
import { useSession } from "next-auth/react";
import { clearSession, setSession } from "../model/auth-store";

export function AuthBootstrap() {
  const pathname = usePathname();
  const router = useRouter();
  const { data: session, status } = useSession();

  useEffect(() => {
    if (status === "loading") return;

    if (status === "authenticated" && session?.accessToken) {
      setSession(session.accessToken, session.roles, session.user?.name ?? null);
      if (pathname === "/login") router.replace("/");
      return;
    }

    clearSession();
    if (pathname !== "/login") router.replace("/login");
  }, [pathname, router, session, status]);

  return null;
}
