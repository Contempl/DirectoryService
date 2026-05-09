"use client"

import { Input } from "@/shared/components/ui/input";
import { Search } from "lucide-react";
import { useEffect, useState } from "react";
import { useDebounce } from "use-debounce";
import { DepartmentListId, setDepartmentSearch, useDepartmentSearch } from "./model/department-list-store";

type DepartmentsFilterProps = {
  stateId?: DepartmentListId;
};

export function DepartmentsFilter({ stateId }: DepartmentsFilterProps) {
  const search = useDepartmentSearch(stateId);

  const [localSearch, setLocalSearch] = useState<string>(search ?? "");
  const [debouncedSearch] = useDebounce(localSearch, 300);

  useEffect(() => {
    setDepartmentSearch(debouncedSearch, stateId);
  }, [debouncedSearch, stateId]);

  return (
    <div className="relative flex-1">
      <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-muted-foreground" />
      <Input
        value={localSearch}
        onChange={(e) => setLocalSearch(e.target.value)}
        placeholder="Поиск..."
        className="pl-9"
      />
    </div>
  );
}