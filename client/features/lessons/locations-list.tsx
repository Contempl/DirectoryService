import { locationsApi } from "@/entities/locations/api";
import type { LocationDto } from "@/entities/locations/types";
import LocationCard from "@/entities/locations/ui/LocationCard";
import { useEffect, useState } from "react";

export default function LocationsList() {
  const [locations, setLocations] = useState<LocationDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const fetch = async () => {
      setLoading(true);
      try {
      //   const result = await locationsApi.getLocations({
      //   page: 1,
      //   pageSize: 10,
      //   isActive: true
      // });
      const result = { data: mockLocations, totalCount: mockLocations.length };
        setLocations(result.data);
      } catch (e: unknown) {
        if (e instanceof Error)
        setError(e.message || "Failed to fetch locations");
      } finally {
        setLoading(false);
      }
    };

    fetch();
  }, []);

  if (loading) return <p>Loading...</p>;
  if (error) return <p>Error: {error}</p>;
  if (locations.length === 0) return <p>No locations</p>;

  return (
    <div>
      <h2>Locations ({locations.length})</h2>
      <div
        style={{
          display: "grid",
          gap: 12,
          gridTemplateColumns: "repeat(auto-fill, minmax(200px, 1fr))",
        }}
      >
        {locations.map((loc) => (
          <LocationCard key={loc.id} location={loc} />
        ))}
      </div>
    </div>
  );
}

export const mockLocations: LocationDto[] = [
  {
    id: "1",
    name: "Main Office",
    address: { city: "Berlin", street: "Alexanderplatz", house: "1" },
    timezone: "Europe/Berlin",
    isActive: true,
    createdAt: new Date().toISOString(),
    updatedAt: new Date().toISOString(),
  },
  {
    id: "2",
    name: "Branch Office",
    address: { city: "Munich", street: "Marienplatz", house: "5" },
    timezone: "Europe/Berlin",
    isActive: false,
    createdAt: new Date().toISOString(),
    updatedAt: null,
  },
];