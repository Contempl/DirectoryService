import { create } from "zustand";

export type UploadStatus = "idle" | "uploading" | "success" | "error";

type UploadState = {
  status: UploadStatus;
  uploadedChunks: number;
  totalChunks: number;
  error: string | null;
  mediaAssetId: string | null;
};

type UploadActions = {
  reset: () => void;
  _setUploading: (totalChunks: number) => void;
  _setChunkUploaded: (uploadedChunks: number) => void;
  _setSuccess: (mediaAssetId: string) => void;
  _setError: (error: string) => void;
};

const initialState: UploadState = {
  status: "idle",
  uploadedChunks: 0,
  totalChunks: 0,
  error: null,
  mediaAssetId: null,
};

export const useUploadStore = create<UploadState & UploadActions>((set) => ({
  ...initialState,
  reset: () => set(initialState),
  _setUploading: (totalChunks) =>
    set({ status: "uploading", uploadedChunks: 0, totalChunks, error: null }),
  _setChunkUploaded: (uploadedChunks) => set({ uploadedChunks }),
  _setSuccess: (mediaAssetId) => set({ status: "success", mediaAssetId }),
  _setError: (error) => set({ status: "error", error }),
}));
