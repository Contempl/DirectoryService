"use client";

import { usePathname, useRouter, useSearchParams } from "next/navigation";
import { useCallback } from "react";

export type LocationsView = "active" | "archived";

export function useLocationsView() {
  const router = useRouter();
  const pathname = usePathname();
  const searchParams = useSearchParams();
  const view: LocationsView = searchParams.get("view") === "archived" ? "archived" : "active";

  const setView = useCallback((nextView: LocationsView) => {
    const params = new URLSearchParams(searchParams.toString());
    if (nextView === "archived") params.set("view", "archived");
    else params.delete("view");
    const query = params.toString();
    router.replace(query ? `${pathname}?${query}` : pathname, { scroll: false });
  }, [pathname, router, searchParams]);

  return { view, setView, isArchived: view === "archived" };
}
