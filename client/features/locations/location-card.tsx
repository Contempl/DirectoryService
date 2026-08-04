import type { LocationDto } from "@/entities/locations/types";
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
import { Button } from "@/shared/components/ui/button";
import { RotateCcw, Trash2 } from "lucide-react";
import { useDeleteLocation } from "./model/use-delete-location";
import { useRestoreLocation } from "./model/use-restore-location";

interface Props {
  location: LocationDto;
  onEdit: () => void;
  archived?: boolean;
}

export default function LocationCard({ location, onEdit, archived = false }: Props) {
  const { deleteLocation, isPending: isDeleting } = useDeleteLocation();
  const { restoreLocation, isPending: isRestoring } = useRestoreLocation();

  return (
    <div className="flex flex-col gap-2 rounded-lg border border-gray-300 bg-white p-4 shadow-sm transition-shadow hover:shadow-md">
      <h3 className="text-lg font-semibold text-gray-800">{location.name}</h3>
      <p className="text-sm text-gray-600">
        {location.address.city}, {location.address.street}
      </p>
      <p className={`text-sm font-medium ${location.isActive ? "text-green-600" : "text-gray-500"}`}>
        {location.isActive ? "Активная" : "Архивная"}
      </p>
      <p className="text-xs text-gray-400">
        Создана: {new Date(location.createdAt).toLocaleDateString()}
      </p>

      {archived && location.updatedAt ? (
        <p className="text-xs text-gray-500">
          Удалена: {new Date(location.updatedAt).toLocaleString()}
        </p>
      ) : location.updatedAt ? (
        <p className="text-xs text-gray-400">
          Обновлена: {new Date(location.updatedAt).toLocaleString()}
        </p>
      ) : null}

      {archived ? (
        <AlertDialog>
          <AlertDialogTrigger asChild>
            <Button className="mt-4" variant="outline" disabled={isRestoring}>
              <RotateCcw className="h-4 w-4" />
              Восстановить
            </Button>
          </AlertDialogTrigger>
          <AlertDialogContent>
            <AlertDialogHeader>
              <AlertDialogTitle>Восстановить локацию?</AlertDialogTitle>
              <AlertDialogDescription>
                Локация «{location.name}» вернётся в список активных.
              </AlertDialogDescription>
            </AlertDialogHeader>
            <AlertDialogFooter>
              <AlertDialogCancel>Отмена</AlertDialogCancel>
              <AlertDialogAction onClick={() => void restoreLocation(location.id)}>
                Восстановить
              </AlertDialogAction>
            </AlertDialogFooter>
          </AlertDialogContent>
        </AlertDialog>
      ) : (
        <div className="mt-4 flex items-center justify-between gap-2">
          <Button onClick={onEdit}>Редактировать</Button>
          <Button
            variant="ghost"
            size="icon"
            className="h-8 w-8 text-destructive transition-colors hover:bg-red-500! hover:text-white!"
            onClick={() => void deleteLocation(location.id)}
            disabled={isDeleting}
            aria-label={`Удалить локацию ${location.name}`}
          >
            <Trash2 className="h-4 w-4" />
          </Button>
        </div>
      )}
    </div>
  );
}
