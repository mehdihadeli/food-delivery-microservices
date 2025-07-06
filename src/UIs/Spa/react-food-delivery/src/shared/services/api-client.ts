import type { ProblemDetails } from "@shared/models/problem-details";
import axios, {
  AxiosError,
  type AxiosInstance,
  type AxiosRequestConfig,
  type AxiosResponse,
} from "axios";
import { toast } from "react-toastify";

export class ApiClient {
  protected readonly client: AxiosInstance;

  constructor(baseURL: string = "/") {
    this.client = axios.create({
      baseURL,
      withCredentials: true,
      headers: {
        "Content-Type": "application/json",
        "X-CSRF": "1",
      },
    });

    this.client.interceptors.response.use(
      (response) => response,
      (error: AxiosError<ProblemDetails>) => {
        const problemDetails = this.getProblemDetails(error);
        this.showErrorToast(problemDetails);
        return Promise.reject(problemDetails);
      }
    );
  }

  protected async get<T>(url: string, config?: AxiosRequestConfig): Promise<T> {
    const response = await this.client.get<T>(url, config);
    return response.data;
  }

  protected async post<T>(
    url: string,
    data?: unknown,
    config?: AxiosRequestConfig
  ): Promise<T> {
    const response = await this.client.post<T>(url, data, config);
    return response.data;
  }

  protected async put<T>(
    url: string,
    data?: unknown,
    config?: AxiosRequestConfig
  ): Promise<T> {
    const response = await this.client.put<T>(url, data, config);
    return response.data;
  }

  protected async delete<T>(
    url: string,
    config?: AxiosRequestConfig
  ): Promise<T> {
    const response = await this.client.delete<T>(url, config);
    return response.data;
  }

  private getProblemDetails(error: AxiosError<ProblemDetails>): ProblemDetails {
    if (error.response && this.isProblemDetails(error.response)) {
      return error.response.data;
    }

    return {
      type: `https://httpstatuses.com/${error.response?.status || 500}`,
      title: error.response?.statusText || "Internal Server Error",
      status: error.response?.status,
      detail: error.message,
      instance: error.config?.url,
    };
  }

  private showErrorToast(problemDetails: ProblemDetails): void {
    // Show the title if available, otherwise show a generic error
    const message = problemDetails.title || "An error occurred";

    toast.error(message, {
      position: "top-right",
      autoClose: 5000,
      hideProgressBar: false,
      closeOnClick: true,
      pauseOnHover: true,
      draggable: true,
      progress: undefined,
    });
  }

  private isProblemDetails(response: AxiosResponse): boolean {
    const contentType = response.headers["content-type"];
    return (
      contentType?.includes("application/problem+json") ||
      (response.data &&
        typeof response.data === "object" &&
        ("type" in response.data || "title" in response.data))
    );
  }
}
