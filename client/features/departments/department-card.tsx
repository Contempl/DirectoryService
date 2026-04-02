"use client"

import { DepartmentShortDto } from "@/entities/departments/types";
import { Checkbox } from "@/shared/components/ui/checkbox";

type DepartmentCardProps = {
  department: DepartmentShortDto;
  isSelected: boolean;
  onToggle: (department: DepartmentShortDto) => void;
};

export function DepartmentCard({ department, isSelected, onToggle }: DepartmentCardProps) {
  return (
    <div
      className="flex items-center gap-3 p-3 rounded-lg border cursor-pointer hover:bg-accent transition-colors"
      onClick={() => onToggle(department)}
    >
      <Checkbox
        checked={isSelected}
        onCheckedChange={() => onToggle(department)}
      />
      <div className="flex flex-col flex-1 min-w-0">
        <span className="font-medium truncate">{department.name}</span>
        <span className="text-sm text-muted-foreground truncate">{department.path}</span>
      </div>
      {department.isActive ? (
        <span className="text-xs px-2 py-0.5 rounded-full bg-green-100 text-green-700">
          Активно
        </span>
      ) : (
        <span className="text-xs px-2 py-0.5 rounded-full bg-gray-100 text-gray-500">
          Неактивно
        </span>
      )}
    </div>
  );
}