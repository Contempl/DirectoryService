"use client";

import { departmentsApi } from "@/entities/departments/api";
import type { DepartmentShortDto, DepartmentWithChildrenDto } from "@/entities/departments/types";
import { isEnvelopeError } from "@/shared/api/errors";
import { Button } from "@/shared/components/ui/button";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/shared/components/ui/dialog";
import { Label } from "@/shared/components/ui/label";
import { useQuery } from "@tanstack/react-query";
import { useState } from "react";
import { DepartmentsPicker } from "./departments-picker";
import { useMoveDepartment } from "./model/use-move-department";

type Destination = "department" | "root" | null;

type MoveDepartmentDialogProps = {
  department: DepartmentWithChildrenDto;
  currentParentName: string | null;
  open: boolean;
  onOpenChange: (open: boolean) => void;
};

const operationErrorMessages: Record<string, string> = {
  "department.move.cycle": "Нельзя перенести подразделение внутрь самого себя или его дочернего подразделения.",
  "department.move.parent_deleted": "Выбранное родительское подразделение удалено. Выберите другое.",
  "department.move.not_found": "Подразделение не найдено. Обновите страницу и попробуйте снова.",
};

const getErrorText = (code: string, fallback: string) =>
  operationErrorMessages[code] ?? fallback;

export function MoveDepartmentDialog({
  department,
  currentParentName,
  open,
  onOpenChange,
}: MoveDepartmentDialogProps) {
  const [destination, setDestination] = useState<Destination>(null);
  const [selectedDepartments, setSelectedDepartments] = useState<DepartmentShortDto[]>([]);
  const moveMutation = useMoveDepartment();

  const descendantsQuery = useQuery({
    queryKey: ["departments", department.id, "descendant-ids"],
    queryFn: () => departmentsApi.getDescendantIds(department.id),
    enabled: open,
  });

  const reset = () => {
    setDestination(null);
    setSelectedDepartments([]);
    moveMutation.reset();
  };

  const handleOpenChange = (nextOpen: boolean) => {
    if (!nextOpen) reset();
    onOpenChange(nextOpen);
  };

  const handleDepartmentChange = (selected: DepartmentShortDto[]) => {
    setSelectedDepartments(selected);
    setDestination(selected.length ? "department" : null);
    moveMutation.reset();
  };

  const selectRoot = () => {
    setSelectedDepartments([]);
    setDestination("root");
    moveMutation.reset();
  };

  const selectedParent = selectedDepartments[0];
  const isSameParent =
    (destination === "root" && department.parentId === null) ||
    (destination === "department" && selectedParent?.id === department.parentId);

  const messages = isEnvelopeError(moveMutation.error) ? moveMutation.error.messages : [];
  const fieldError = messages.find((message) =>
    message.invalidField?.toLowerCase().includes("parent")
  );
  const operationError = messages.find((message) => message !== fieldError);
  const operationErrorText = operationError
    ? getErrorText(operationError.code, operationError.message)
    : moveMutation.isError && !fieldError
      ? "Не удалось перенести подразделение. Попробуйте ещё раз."
      : null;

  const submit = async () => {
    if (!destination || isSameParent) return;
    try {
      await moveMutation.mutateAsync({
        departmentId: department.id,
        parentId: destination === "root" ? null : selectedParent.id,
      });
      handleOpenChange(false);
    } catch {
      // The mutation state renders the backend error inside the dialog.
    }
  };

  return (
    <Dialog open={open} onOpenChange={handleOpenChange}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>Перенести «{department.name}»</DialogTitle>
          <DialogDescription>
            Текущий родитель: {currentParentName ?? "Корень"}. Выберите новое положение в оргструктуре.
          </DialogDescription>
        </DialogHeader>

        <div className="grid gap-3">
          <Label>Новый родитель</Label>
          <DepartmentsPicker
            selectedDepartments={selectedDepartments}
            onChange={handleDepartmentChange}
            excludeIds={[department.id, ...(descendantsQuery.data ?? [])]}
          />
          <Button
            type="button"
            variant={destination === "root" ? "secondary" : "outline"}
            onClick={selectRoot}
          >
            Перенести в корень
          </Button>
          {descendantsQuery.isError && (
            <p className="text-sm text-destructive">Не удалось загрузить список дочерних подразделений.</p>
          )}
          {fieldError && (
            <p className="text-sm text-destructive">
              {getErrorText(fieldError.code, fieldError.message)}
            </p>
          )}

          {destination && (
            <div className="rounded-md border bg-muted/40 p-3 text-sm">
              <span className="font-medium">Предпросмотр: </span>
              «{department.name}» переедет из «{currentParentName ?? "Корень"}» в «{destination === "root" ? "Корень" : selectedParent?.name}».
              {isSameParent && <p className="mt-1 text-muted-foreground">Это подразделение уже находится здесь.</p>}
            </div>
          )}

          {operationErrorText && (
            <p className="rounded-md border border-destructive/30 bg-destructive/10 p-3 text-sm text-destructive">
              {operationErrorText}
            </p>
          )}
        </div>

        <DialogFooter>
          <Button type="button" variant="outline" onClick={() => handleOpenChange(false)}>
            Отмена
          </Button>
          <Button
            type="button"
            disabled={!destination || isSameParent || descendantsQuery.isPending || moveMutation.isPending}
            onClick={() => void submit()}
          >
            {moveMutation.isPending ? "Переносим…" : "Перенести"}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
