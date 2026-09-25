"use client";

import { useEffect, useRef } from "react";
import { usePathname, useRouter } from "next/navigation";
import { refreshAccessToken } from "../model/auth-session";

export function AuthBootstrap() {
  const pathname = usePathname();
  const router = useRouter();
  const initialized = useRef(false);

  useEffect(() => {
    if (initialized.current) return;
    initialized.current = true;

    void refreshAccessToken()
      .then(() => {
        if (pathname === "/login") router.replace("/");
      })
      .catch(() => {
        if (pathname !== "/login") router.replace("/login");
      });
  }, [pathname, router]);

  return null;
}
