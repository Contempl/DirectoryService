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
import { Checkbox } from "@/shared/components/ui/checkbox";

interface Props {
  location: LocationDto;
  onEdit: () => void;
  archived?: boolean;
  selected?: boolean;
  onSelectedChange?: (selected: boolean) => void;
}

export default function LocationCard({
  location,
  onEdit,
  archived = false,
  selected = false,
  onSelectedChange,
}: Props) {
  const { deleteLocation, isPending: isDeleting } = useDeleteLocation();
  const { restoreLocation, isPending: isRestoring } = useRestoreLocation();

  return (
    <div className={`relative flex flex-col gap-2 rounded-lg border bg-white p-4 shadow-sm transition-shadow hover:shadow-md ${selected ? "border-primary ring-2 ring-primary/20" : "border-gray-300"}`}>
      <div className="absolute right-4 top-4 flex items-center gap-2">
        <label htmlFor={`select-location-${location.id}`} className="text-xs text-gray-500">
          Выбрать
        </label>
        <Checkbox
          id={`select-location-${location.id}`}
          checked={selected}
          onCheckedChange={(checked) => onSelectedChange?.(checked === true)}
          aria-label={`Выбрать локацию ${location.name}`}
        />
      </div>
      <h3 className="pr-24 text-lg font-semibold text-gray-800">{location.name}</h3>
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
          <AlertDialog>
            <AlertDialogTrigger asChild>
              <Button
                variant="ghost"
                size="icon"
                className="h-8 w-8 text-destructive transition-colors hover:bg-red-500! hover:text-white!"
                disabled={isDeleting}
                aria-label={`Удалить локацию ${location.name}`}
              >
                <Trash2 className="h-4 w-4" />
              </Button>
            </AlertDialogTrigger>
            <AlertDialogContent>
              <AlertDialogHeader>
                <AlertDialogTitle>Переместить локацию в архив?</AlertDialogTitle>
                <AlertDialogDescription>
                  Локация «{location.name}» исчезнет из активного списка. Её можно будет восстановить из архива.
                </AlertDialogDescription>
              </AlertDialogHeader>
              <AlertDialogFooter>
                <AlertDialogCancel>Отмена</AlertDialogCancel>
                <AlertDialogAction
                  variant="destructive"
                  onClick={() => void deleteLocation(location.id)}
                  disabled={isDeleting}
                >
                  Переместить в архив
                </AlertDialogAction>
              </AlertDialogFooter>
            </AlertDialogContent>
          </AlertDialog>
        </div>
      )}
    </div>
  );
}
