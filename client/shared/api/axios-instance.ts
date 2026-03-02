import axios from "axios";
import { Envelope } from "./envelope";
import { EnvelopeError } from "./errors";

const API_BASE_URL = process.env.REACT_APP_API_URL || 'http://localhost:5111/api';

export const apiClient = axios.create({
    baseURL: API_BASE_URL,
    timeout: 10000,
    headers: {
        'Content-Type': 'application/json',
    },
});

apiClient.interceptors.response.use(
    (response) => {
        const data = response.data as Envelope;

        if (data.isError && data.error) {
            throw new EnvelopeError(data.error);
        }

        return response;
    },
    (error) => {
        if(axios.isAxiosError(error) && error.response?.data) {
            const envelope = error.response.data as Envelope;

            if (envelope?.isError && envelope.error) {
                throw new EnvelopeError(envelope.error);
            }
        }

        return Promise.reject(error);
    }
);