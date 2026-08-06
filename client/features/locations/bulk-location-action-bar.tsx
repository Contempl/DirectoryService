"use client";

import { Archive, Loader2, RotateCcw, X } from "lucide-react";
import { useState } from "react";

import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
} from "@/shared/components/ui/alert-dialog";
import { Button } from "@/shared/components/ui/button";
import { useBulkUpdateLocationsActivity } from "./model/use-bulk-update-locations-activity";

type Props = {
  selectedIds: string[];
  restoring: boolean;
  onClear: () => void;
};

export function BulkLocationActionBar({ selectedIds, restoring, onClear }: Props) {
  const [confirmOpen, setConfirmOpen] = useState(false);
  const { updateLocationsActivity, isPending } = useBulkUpdateLocationsActivity();
  const count = selectedIds.length;
  const action = restoring ? "Восстановить" : "Архивировать";

  const confirm = async () => {
    try {
      await updateLocationsActivity({ locationIds: selectedIds, isActive: restoring });
      setConfirmOpen(false);
      onClear();
    } catch {
      // Mutation hook reports the request error and keeps the selection for retry.
    }
  };

  return (
    <>
      <div className="sticky bottom-4 z-30 flex flex-wrap items-center justify-between gap-3 rounded-lg border bg-background p-3 shadow-lg">
        <div>
          <p className="font-medium">Выбрано локаций: {count}</p>
          <p className="text-xs text-muted-foreground">Только загруженные видимые карточки</p>
        </div>
        <div className="flex items-center gap-2">
          <Button variant="ghost" onClick={onClear} disabled={isPending}>
            <X className="size-4" /> Снять выбор
          </Button>
          <Button
            variant={restoring ? "default" : "destructive"}
            onClick={() => setConfirmOpen(true)}
            disabled={isPending}
          >
            {isPending ? <Loader2 className="size-4 animate-spin" /> : restoring ? <RotateCcw className="size-4" /> : <Archive className="size-4" />}
            {action}
          </Button>
        </div>
      </div>

      <AlertDialog open={confirmOpen} onOpenChange={(open) => !isPending && setConfirmOpen(open)}>
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>{action} выбранные локации?</AlertDialogTitle>
            <AlertDialogDescription>
              Будет обработано локаций: {count}. Операция отправится одним запросом.
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogCancel disabled={isPending}>Отмена</AlertDialogCancel>
            <AlertDialogAction
              variant={restoring ? "default" : "destructive"}
              disabled={isPending}
              onClick={(event) => {
                event.preventDefault();
                void confirm();
              }}
            >
              {isPending && <Loader2 className="size-4 animate-spin" />}
              {action} {count}
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>
    </>
  );
}
