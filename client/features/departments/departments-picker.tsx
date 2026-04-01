"use client"

import { DepartmentShortDto } from "@/entities/departments/types";
import { Badge } from "@/shared/components/ui/badge";
import { Button } from "@/shared/components/ui/button";
import { Dialog, DialogContent, DialogHeader, DialogTitle } from "@/shared/components/ui/dialog";
import { X } from "lucide-react";
import { useState } from "react";
import { DepartmentsFilter } from "./departments-filter";
import { DepartmentsList } from "./deaprtments-list";


type DepartmentsPickerProps = {
  selectedDepartments: DepartmentShortDto[];
  onChange: (selected: DepartmentShortDto[]) => void;
  multiselect?: boolean;
  excludeIds?: string[];
};

const PICKER_STATE_ID = "departments-picker";

export function DepartmentsPicker({
  selectedDepartments,
  onChange,
  multiselect = false,
  excludeIds,
}: DepartmentsPickerProps) {
  const [open, setOpen] = useState(false);

  const handleToggle = (department: DepartmentShortDto) => {
    const isSelected = selectedDepartments.some((s) => s.id === department.id);

    if (isSelected) {
      onChange(selectedDepartments.filter((s) => s.id !== department.id));
    } else {
      if (multiselect) {
        onChange([...selectedDepartments, department]);
      } else {
        onChange([department]);
        setOpen(false);
      }
    }
  };

  const handleRemove = (id: string) => {
    onChange(selectedDepartments.filter((s) => s.id !== id));
  };

  return (
    <div className="flex flex-col gap-2">
      {/* выбранные подразделения */}
      {selectedDepartments.length > 0 && (
        <div className="flex flex-wrap gap-2">
          {selectedDepartments.map((d) => (
            <Badge key={d.id} variant="secondary" className="flex items-center gap-1">
              {d.name}
              <X
                className="h-3 w-3 cursor-pointer"
                onClick={() => handleRemove(d.id)}
              />
            </Badge>
          ))}
        </div>
      )}

      <Button variant="outline" onClick={() => setOpen(true)}>
        Выбрать подразделения
      </Button>

      <Dialog open={open} onOpenChange={setOpen}>
        <DialogContent className="max-w-lg">
          <DialogHeader>
            <DialogTitle>Выбор подразделений</DialogTitle>
          </DialogHeader>

          <DepartmentsFilter stateId={PICKER_STATE_ID} />

          <div className="overflow-y-auto max-h-[400px] mt-2">
            <DepartmentsList
              selectedDepartments={selectedDepartments}
              onToggle={handleToggle}
              excludeIds={excludeIds}
              stateId={PICKER_STATE_ID}
            />
          </div>

          <div className="flex justify-end mt-2">
            <Button variant="ghost" onClick={() => setOpen(false)}>
              Отмена
            </Button>
          </div>
        </DialogContent>
      </Dialog>
    </div>
  );
}