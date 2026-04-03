"use client"

import { PositionDto } from "@/entities/positions/types";
import { Dialog, DialogContent, DialogHeader, DialogTitle } from "@/shared/components/ui/dialog";
import { EditPositionForm } from "./edit-position-form";

type EditPositionDialogProps = {
  position: PositionDto;
  open: boolean;
  onOpenChange: (open: boolean) => void;
};

export function EditPositionDialog({ position, open, onOpenChange }: EditPositionDialogProps) {
  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="max-w-lg">
        <DialogHeader>
          <DialogTitle>Редактировать позицию</DialogTitle>
        </DialogHeader>
        <EditPositionForm
          position={position}
          onSuccess={() => onOpenChange(false)}
        />
      </DialogContent>
    </Dialog>
  );
}