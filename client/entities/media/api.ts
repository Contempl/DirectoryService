import axios from "axios";
import type { Envelope } from "@/shared/api/envelope";
import type {
  CancelMultipartUploadRequest,
  CancelMultipartUploadResponse,
  CompleteMultipartUploadRequest,
  CompleteMultipartUploadResponse,
  StartMultipartUploadRequest,
  StartMultipartUploadResponse,
  MediaAssetInfo,
  VideoProcessingStatus,
} from "./types";

const FILE_SERVICE_BASE_URL = process.env.NEXT_PUBLIC_FILE_SERVICE_URL
  ?? (process.env.NODE_ENV === "production" ? "/api" : "http://localhost:5555/api");

const fileServiceClient = axios.create({
  baseURL: FILE_SERVICE_BASE_URL,
  timeout: 15_000,
  headers: { "Content-Type": "application/json" },
});

type FileServiceEnvelope<T = unknown> = Omit<Envelope<T>, "error"> & {
  errorsList: Array<{
    code: string;
    message: string;
    type: string | number;
    invalidField?: string | null;
  }> | null;
};

function getEnvelopeMessage(envelope: FileServiceEnvelope): string | null {
  return envelope.errorsList?.[0]?.message ?? null;
}

function unwrap<T>(envelope: FileServiceEnvelope<T>): T {
  const message = getEnvelopeMessage(envelope);
  if (envelope.isError || message) {
    throw new Error(message ?? "File Service returned an error.");
  }

  if (envelope.result === null) {
    throw new Error("File Service returned no result.");
  }

  return envelope.result;
}

fileServiceClient.interceptors.response.use(
  (response) => {
    const data = response.data as FileServiceEnvelope;
    const message = getEnvelopeMessage(data);
    if (data.isError || message) throw new Error(message ?? "File Service returned an error.");
    return response;
  },
  (error) => {
    if (axios.isAxiosError(error) && error.response?.data) {
      const envelope = error.response.data as FileServiceEnvelope;
      const message = getEnvelopeMessage(envelope);
      if (envelope?.isError || message) throw new Error(message ?? "File Service request failed.");
    }
    return Promise.reject(error);
  }
);

export const mediaApi = {
  startMultipartUpload: async (
    request: StartMultipartUploadRequest,
    signal?: AbortSignal
  ): Promise<StartMultipartUploadResponse> => {
    const response = await fileServiceClient.post<FileServiceEnvelope<StartMultipartUploadResponse>>(
      "/files/multipart/start",
      request,
      { signal }
    );
    return unwrap(response.data);
  },

  completeMultipartUpload: async (
    request: CompleteMultipartUploadRequest,
    signal?: AbortSignal
  ): Promise<CompleteMultipartUploadResponse> => {
    const response = await fileServiceClient.post<FileServiceEnvelope<CompleteMultipartUploadResponse>>(
      "/files/multipart/complete",
      request,
      { signal }
    );
    return unwrap(response.data);
  },

  cancelMultipartUpload: async (
    request: CancelMultipartUploadRequest,
    signal?: AbortSignal
  ): Promise<CancelMultipartUploadResponse> => {
    const response = await fileServiceClient.post<FileServiceEnvelope<CancelMultipartUploadResponse>>(
      "/files/multipart/cancel",
      request,
      { signal }
    );
    return unwrap(response.data);
  },

  getMediaAsset: async (mediaAssetId: string): Promise<MediaAssetInfo> => {
    const response = await fileServiceClient.get<FileServiceEnvelope<MediaAssetInfo>>(
      `/files/${mediaAssetId}`
    );
    return unwrap(response.data);
  },

  getVideoProcessingStatus: async (videoAssetId: string): Promise<VideoProcessingStatus> => {
    const response = await fileServiceClient.get<FileServiceEnvelope<VideoProcessingStatus>>(
      `/files/${videoAssetId}/processing-status`
    );
    return unwrap(response.data);
  },
};
