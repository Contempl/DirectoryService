import { LocationDto } from "../../entities/locations/types";


interface Props {
  location: LocationDto;
}

export default function LocationCard({ location }: Props) {
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
    </div>
  );
}