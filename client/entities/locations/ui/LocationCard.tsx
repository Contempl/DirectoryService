import { LocationDto } from "../types";


interface Props {
  location: LocationDto;
}

export default function LocationCard({ location }: Props) {
  return (
    <div
      key={location.id}
      style={{ border: "1px solid #ddd", padding: 12, borderRadius: 8 }}
    >
      <h3>{location.name}</h3>
      <p>
        {location.address.city} {location.address.street}
      </p>
      <p>Status: {location.isActive ? "Active" : "Inactive"}</p>
    </div>
  );
}