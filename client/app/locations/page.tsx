import LocationsList from "@/features/locations/locations-list";
import { Suspense } from "react";

export default function LocationsPage() {
  return (
    <Suspense fallback={<p>Loading locations...</p>}>
      <LocationsList />
    </Suspense>
  );
}
