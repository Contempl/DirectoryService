export type AddressDto = {
  city: string;
  street: string;
  house: string;
  apartment?: string | null;
}

export type LocationDto = {
  id: string; 
  name: string;
  address: AddressDto;
  timezone: string;
  isActive: boolean;
  createdAt: string; 
  updatedAt?: string | null;
}