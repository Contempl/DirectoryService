"use client"

import { PositionDto } from "@/entities/positions/types";
import { Button } from "@/shared/components/ui/button";
import { Trash2 } from "lucide-react";
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
  AlertDialogTrigger,
} from "@/shared/components/ui/alert-dialog";

type PositionCardProps = {
  position: PositionDto;
  onEdit: () => void;
  onDelete: () => void;
};

export function PositionCard({ position, onEdit, onDelete }: PositionCardProps) {
  return (
    <div className="rounded-lg border p-4 flex flex-col gap-2">
      <div className="flex items-center justify-between">
        <span className="font-semibold text-lg">{position.name}</span>
        {position.isActive ? (
          <span className="text-xs px-2 py-0.5 rounded-full bg-green-100 text-green-700">
            Активна
          </span>
        ) : (
          <span className="text-xs px-2 py-0.5 rounded-full bg-gray-100 text-gray-500">
            Неактивна
          </span>
        )}
      </div>

      {position.description && (
        <p className="text-sm text-muted-foreground">{position.description}</p>
      )}

      <p className="text-sm text-muted-foreground">
        Подразделений: {position.departmentsCount}
      </p>

      <div className="flex items-center justify-between mt-2">
        <Button variant="outline" onClick={onEdit}>
          Изменить
        </Button>

        <AlertDialog>
          <AlertDialogTrigger asChild>
            <Button variant="destructive" size="icon">
              <Trash2 className="h-4 w-4" />
            </Button>
          </AlertDialogTrigger>
          <AlertDialogContent>
            <AlertDialogHeader>
              <AlertDialogTitle>Удалить позицию?</AlertDialogTitle>
              <AlertDialogDescription>
                Вы уверены что хотите удалить позицию «{position.name}»? Это действие необратимо.
              </AlertDialogDescription>
            </AlertDialogHeader>
            <AlertDialogFooter>
              <AlertDialogCancel>Отмена</AlertDialogCancel>
              <AlertDialogAction onClick={onDelete}>Удалить</AlertDialogAction>
            </AlertDialogFooter>
          </AlertDialogContent>
        </AlertDialog>
      </div>
    </div>
  );
}