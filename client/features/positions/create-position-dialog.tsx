"use client"

import { Dialog, DialogContent, DialogHeader, DialogTitle } from "@/shared/components/ui/dialog";
import { CreatePositionForm } from "./create-position-form";

type CreatePositionDialogProps = {
  open: boolean;
  onOpenChange: (open: boolean) => void;
};

export function CreatePositionDialog({ open, onOpenChange }: CreatePositionDialogProps) {
  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="max-w-lg">
        <DialogHeader>
          <DialogTitle>Создать позицию</DialogTitle>
        </DialogHeader>
        <CreatePositionForm onSuccess={() => onOpenChange(false)} />
      </DialogContent>
    </Dialog>
  );
}