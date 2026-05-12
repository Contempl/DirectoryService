import axios from "axios";
import type { Envelope } from "@/shared/api/envelope";
import { EnvelopeError } from "@/shared/api/errors";
import type {
  CancelMultipartUploadRequest,
  CompleteMultipartUploadRequest,
  CompleteMultipartUploadResponse,
  StartMultipartUploadRequest,
  StartMultipartUploadResponse,
} from "./types";

const fileServiceClient = axios.create({
  baseURL: process.env.NEXT_PUBLIC_FILE_SERVICE_URL || "http://localhost:5555",
  headers: { "Content-Type": "application/json" },
});

fileServiceClient.interceptors.response.use(
  (response) => {
    const data = response.data as Envelope;
    if (data.isError && data.error) throw new EnvelopeError(data.error);
    return response;
  },
  (error) => {
    if (axios.isAxiosError(error) && error.response?.data) {
      const envelope = error.response.data as Envelope;
      if (envelope?.isError && envelope.error) throw new EnvelopeError(envelope.error);
    }
    return Promise.reject(error);
  }
);

export const mediaApi = {
  startMultipartUpload: async (
    request: StartMultipartUploadRequest
  ): Promise<StartMultipartUploadResponse> => {
    const response = await fileServiceClient.post<Envelope<StartMultipartUploadResponse>>(
      "/files/multipart/start",
      request
    );
    return response.data.result!;
  },

  completeMultipartUpload: async (
    request: CompleteMultipartUploadRequest
  ): Promise<CompleteMultipartUploadResponse> => {
    const response = await fileServiceClient.post<Envelope<CompleteMultipartUploadResponse>>(
      "/files/multipart/complete",
      request
    );
    return response.data.result!;
  },

  cancelMultipartUpload: async (request: CancelMultipartUploadRequest): Promise<void> => {
    await fileServiceClient.post("/files/multipart/cancel", request);
  },
};
