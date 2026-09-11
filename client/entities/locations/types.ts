export type AddressDto = {
  city: string;
  street: string;
  house: string;
  apartment?: string | null;
}

export type LocationPhotoStatus =
  | "available"
  | "temporarily_unavailable"
  | "missing"
  | "deleted";

export type LocationPhotoDto = {
  assetId: string;
  status: LocationPhotoStatus;
  fileName: string;
  contentType: string;
  size: number;
  verifiedAt: string;
  contentUrl: string | null;
};

export type LocationDto = {
  id: string; 
  name: string;
  address: AddressDto;
  timezone: string;
  isActive: boolean;
  createdAt: string; 
  updatedAt?: string | null;
  photo: LocationPhotoDto | null;
}
