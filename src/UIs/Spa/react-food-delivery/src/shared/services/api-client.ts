import { createApi } from '@shared/libs/axios/axios-utils';
import { type AxiosInstance, type AxiosRequestConfig } from 'axios';

export class ApiClient {
	protected readonly api: AxiosInstance;

	constructor(baseURL: string = '/') {
		this.api = createApi(baseURL);
	}

	protected async get<T>(url: string, config?: AxiosRequestConfig): Promise<T> {
		const response = await this.api.get<T>(url, config);
		return response.data;
	}

	protected async post<T>(url: string, data?: unknown, config?: AxiosRequestConfig): Promise<T> {
		const response = await this.api.post<T>(url, data, config);
		return response.data;
	}

	protected async put<T>(url: string, data?: unknown, config?: AxiosRequestConfig): Promise<T> {
		const response = await this.api.put<T>(url, data, config);
		return response.data;
	}

	protected async delete<T>(url: string, config?: AxiosRequestConfig): Promise<T> {
		const response = await this.api.delete<T>(url, config);
		return response.data;
	}
}
