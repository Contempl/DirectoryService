"use client";

import { useState } from "react";
import type { LocationDto, LocationPhotoStatus } from "@/entities/locations/types";
import { FileUpload } from "@/features/file-upload/ui/file-upload";
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
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogHeader,
  DialogTitle,
} from "@/shared/components/ui/dialog";
import { Download, ImageOff, LoaderCircle, RefreshCw, Trash2, Upload } from "lucide-react";
import { useLocationPhoto } from "./model/use-location-photo";

type Props = {
  location: LocationDto;
  disabled?: boolean;
};

const statusText: Record<LocationPhotoStatus, string> = {
  available: "Available",
  temporarily_unavailable: "File service is temporarily unavailable",
  missing: "File is missing",
  deleted: "File was deleted",
};

function formatSize(bytes: number) {
  if (bytes < 1024 * 1024) return `${Math.ceil(bytes / 1024)} KB`;
  return `${(bytes / 1024 / 1024).toFixed(1)} MB`;
}

export function LocationPhotoAttachment({ location, disabled = false }: Props) {
  const [dialogOpen, setDialogOpen] = useState(false);
  const [uploadedAssetId, setUploadedAssetId] = useState<string | null>(null);
  const photo = location.photo;
  const {
    setPhoto,
    resetSetPhoto,
    isSettingPhoto,
    setPhotoError,
    deletePhoto,
    isDeletingPhoto,
    deletePhotoError,
  } = useLocationPhoto(location.id);

  const attach = async (assetId: string) => {
    setUploadedAssetId(assetId);
    try {
      await setPhoto({ assetId, replace: photo !== null });
      setUploadedAssetId(null);
      setDialogOpen(false);
    } catch {
      // The uploaded asset remains available for an explicit retry.
    }
  };

  const handleDialogChange = (open: boolean) => {
    if (isSettingPhoto) return;
    setDialogOpen(open);
    if (!open) {
      setUploadedAssetId(null);
      resetSetPhoto();
    }
  };

  const remove = async () => {
    try {
      await deletePhoto();
    } catch {
      // The mutation error remains visible on the card and the current photo stays intact.
    }
  };

  return (
    <section className="space-y-3 rounded-md border bg-gray-50 p-3">
      <div className="flex gap-3">
        {photo?.status === "available" && photo.contentUrl ? (
          <div
            className="h-20 w-24 shrink-0 rounded-md bg-gray-200 bg-cover bg-center"
            style={{ backgroundImage: `url(${JSON.stringify(photo.contentUrl)})` }}
            role="img"
            aria-label={`Photo for ${location.name}`}
          />
        ) : (
          <div className="flex h-20 w-24 shrink-0 items-center justify-center rounded-md bg-gray-200 text-gray-500">
            <ImageOff className="h-6 w-6" aria-hidden="true" />
          </div>
        )}

        <div className="min-w-0 flex-1">
          <p className="text-sm font-medium">Location photo</p>
          {photo ? (
            <>
              <p className="truncate text-sm text-gray-600">{photo.fileName}</p>
              <p className="text-xs text-gray-500">
                {statusText[photo.status]} · {formatSize(photo.size)}
              </p>
            </>
          ) : (
            <p className="text-sm text-gray-500">No photo attached</p>
          )}
        </div>
      </div>

      <div className="flex flex-wrap gap-2">
        <Dialog open={dialogOpen} onOpenChange={handleDialogChange}>
          <Button type="button" variant="outline" size="sm" disabled={disabled} onClick={() => setDialogOpen(true)}>
            {photo ? <RefreshCw className="mr-2 h-4 w-4" /> : <Upload className="mr-2 h-4 w-4" />}
            {photo ? "Replace" : "Add photo"}
          </Button>
          <DialogContent>
            <DialogHeader>
              <DialogTitle>{photo ? "Replace location photo" : "Add location photo"}</DialogTitle>
              <DialogDescription>
                The new file is attached only after its upload completes successfully.
              </DialogDescription>
            </DialogHeader>
            <FileUpload
              assetType="preview"
              context="location"
              contextId={location.id}
              acceptedTypes={["image/jpeg", "image/png", "image/webp"]}
              maxSizeBytes={10 * 1024 * 1024}
              disabled={isSettingPhoto || uploadedAssetId !== null}
              onSuccess={(asset) => void attach(asset.assetId)}
            />
            {isSettingPhoto && (
              <p className="flex items-center gap-2 text-sm text-gray-600">
                <LoaderCircle className="h-4 w-4 animate-spin" /> Attaching photo to location…
              </p>
            )}
            {setPhotoError && uploadedAssetId && (
              <div className="space-y-2 rounded-md border border-red-300 bg-red-50 p-3 text-sm text-red-700">
                <p>The file was uploaded, but could not be attached to the location.</p>
                <Button type="button" size="sm" variant="outline" onClick={() => void attach(uploadedAssetId)}>
                  Retry attachment
                </Button>
              </div>
            )}
          </DialogContent>
        </Dialog>

        {photo?.status === "available" && photo.contentUrl && (
          <Button asChild type="button" variant="outline" size="sm">
            <a href={photo.contentUrl} target="_blank" rel="noreferrer">
              <Download className="mr-2 h-4 w-4" /> Download
            </a>
          </Button>
        )}

        {photo && (
          <AlertDialog>
            <AlertDialogTrigger asChild>
              <Button type="button" variant="ghost" size="sm" disabled={disabled || isDeletingPhoto}>
                <Trash2 className="mr-2 h-4 w-4" /> Remove
              </Button>
            </AlertDialogTrigger>
            <AlertDialogContent>
              <AlertDialogHeader>
                <AlertDialogTitle>Remove location photo?</AlertDialogTitle>
                <AlertDialogDescription>
                  This removes the photo from the location. It does not delete the uploaded asset.
                </AlertDialogDescription>
              </AlertDialogHeader>
              <AlertDialogFooter>
                <AlertDialogCancel>Cancel</AlertDialogCancel>
                <AlertDialogAction variant="destructive" disabled={isDeletingPhoto} onClick={() => void remove()}>
                  Remove photo
                </AlertDialogAction>
              </AlertDialogFooter>
            </AlertDialogContent>
          </AlertDialog>
        )}
      </div>

      {deletePhotoError && <p className="text-xs text-red-600">Could not remove the photo. Please try again.</p>}
    </section>
  );
}
