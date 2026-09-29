"use client";

import type { ReactNode } from "react";
import { useAuthStore } from "../model/auth-store";

type RoleGateProps = {
  roles: string[];
  children: ReactNode;
  fallback?: ReactNode;
};

export function RoleGate({ roles, children, fallback = null }: RoleGateProps) {
  const userRoles = useAuthStore((state) => state.roles);
  const isAllowed = roles.some((role) =>
    userRoles.some((userRole) => userRole.toLowerCase() === role.toLowerCase()),
  );

  return isAllowed ? children : fallback;
}
