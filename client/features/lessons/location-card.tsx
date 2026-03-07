import { Button } from "@/shared/components/ui/button";
import { LocationDto } from "../../entities/locations/types";
import { Trash2 } from "lucide-react";
import { useDeleteLocation } from "./model/use-delete-location";


interface Props {
  location: LocationDto;
  onEdit: () => void
}

export default function LocationCard({ location, onEdit }: Props) {

const { deleteLocation, isPending } = useDeleteLocation();

  const handleDelete = (e: React.MouseEvent) => {
    e.preventDefault();
    e.stopPropagation();

    deleteLocation(location.id);
  };


  return (
    <div
      key={location.id}
      className="
        border border-gray-300 
        rounded-lg 
        p-4 
        bg-white 
        shadow-sm 
        hover:shadow-md 
        transition-shadow
        flex flex-col
        gap-2
      "
    >
      <h3 className="text-lg font-semibold text-gray-800">{location.name}</h3>
      <p className="text-gray-600 text-sm">
        {location.address.city}, {location.address.street}
      </p>
      <p className={`text-sm font-medium ${location.isActive ? 'text-green-600' : 'text-red-600'}`}>
        {location.isActive ? 'Active' : 'Inactive'}
      </p>
      <p className="text-xs text-gray-400">
        Created: {new Date(location.createdAt).toLocaleDateString()}
      </p>
      {location.updatedAt && (
        <p className="text-xs text-gray-400">
          Updated: {new Date(location.updatedAt).toLocaleDateString()}
        </p>
      )}
      
        <div className="flex items-center justify-between mt-4">
          <span>Обновлено {location.updatedAt?.toLocaleString()}</span>
          <Button
            variant="ghost"
            size="icon"
            className="h-8 w-8 text-destructive hover:text-white! hover:bg-red-500! transition-colors"
            onClick={handleDelete}
            disabled={isPending}
          >
            <Trash2 className="h-4 w-4" />
          </Button>
        </div>

      <button
        onClick={onEdit}
        className="mt-2 px-3 py-1 text-sm bg-blue-600 text-white rounded hover:bg-blue-700"
      >
        Edit
      </button>

    </div>
  );
}