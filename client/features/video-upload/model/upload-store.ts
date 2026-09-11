import { create } from "zustand";

export type UploadStage = "initiate" | "storage" | "complete" | "cancel";
export type UploadStatus = "idle" | "initiating" | "uploading" | "completing" | "success" | "error";

export type UploadFailure = {
  stage: UploadStage;
  message: string;
};

type UploadState = {
  status: UploadStatus;
  uploadedChunks: number;
  totalChunks: number;
  error: UploadFailure | null;
  mediaAssetId: string | null;
};

type UploadActions = {
  reset: () => void;
  _setInitiating: () => void;
  _setUploading: (totalChunks: number) => void;
  _setChunkUploaded: (uploadedChunks: number) => void;
  _setCompleting: () => void;
  _setSuccess: (mediaAssetId: string) => void;
  _setError: (error: UploadFailure) => void;
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
  _setInitiating: () => set({ ...initialState, status: "initiating" }),
  _setUploading: (totalChunks) =>
    set({ status: "uploading", uploadedChunks: 0, totalChunks, error: null }),
  _setChunkUploaded: (uploadedChunks) => set({ uploadedChunks }),
  _setCompleting: () => set({ status: "completing" }),
  _setSuccess: (mediaAssetId) => set({ status: "success", mediaAssetId }),
  _setError: (error) => set({ status: "error", error }),
}));
