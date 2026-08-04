"use client";

import { departmentsQueryOptions } from "@/entities/departments/api";
import type { DepartmentShortDto } from "@/entities/departments/types";
import { useQuery } from "@tanstack/react-query";
import { usePathname, useRouter, useSearchParams } from "next/navigation";
import { useCallback, useMemo } from "react";

const UUID_PATTERN = /^[0-9a-f]{8}-[0-9a-f]{4}-[1-5][0-9a-f]{3}-[89ab][0-9a-f]{3}-[0-9a-f]{12}$/i;

export function useLocationDepartmentFilter() {
  const router = useRouter();
  const pathname = usePathname();
  const searchParams = useSearchParams();

  const departmentIds = useMemo(
    () => Array.from(new Set((searchParams.get("departmentIds") ?? "")
      .split(",")
      .filter((id) => UUID_PATTERN.test(id)))),
    [searchParams]
  );

  const { data: selectedDepartments = [], isPending } = useQuery(
    departmentsQueryOptions.byIds(departmentIds)
  );

  const replaceDepartmentIds = useCallback((ids: string[]) => {
    const params = new URLSearchParams(searchParams.toString());
    if (ids.length > 0) params.set("departmentIds", ids.join(","));
    else params.delete("departmentIds");
    const query = params.toString();
    router.replace(query ? `${pathname}?${query}` : pathname, { scroll: false });
  }, [pathname, router, searchParams]);

  const setSelectedDepartments = useCallback(
    (departments: DepartmentShortDto[]) => replaceDepartmentIds(departments.map(({ id }) => id)),
    [replaceDepartmentIds]
  );

  const reset = useCallback(() => replaceDepartmentIds([]), [replaceDepartmentIds]);

  return {
    departmentIds,
    selectedDepartments,
    setSelectedDepartments,
    reset,
    isActive: departmentIds.length > 0,
    isRestoring: departmentIds.length > 0 && isPending,
  };
}
