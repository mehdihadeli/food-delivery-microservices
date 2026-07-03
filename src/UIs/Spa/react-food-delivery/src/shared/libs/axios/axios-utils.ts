import axios, { AxiosError, type AxiosInstance, type AxiosResponse } from 'axios';
import { toast } from 'react-toastify';
import type { ProblemDetails } from '../problem-details/models/problem-details';
import { ProblemDetailsUtils } from '../problem-details/problem-details-utils';

function setupInterceptors(instance: AxiosInstance): AxiosInstance {
	instance.interceptors.response.use(
		(response: AxiosResponse) => response,
		(error: AxiosError<ProblemDetails>) => {
			const problemDetails = getProblemDetails(error);
			showErrorToast(problemDetails);
			return Promise.reject(problemDetails);
		},
	);
	return instance;
}

export function createApi(baseURL: string = '/'): AxiosInstance {
	const instance = axios.create({
		baseURL,
		withCredentials: true,
		headers: {
			'Content-Type': 'application/json',
			'X-CSRF': '1',
		},
	});

	return setupInterceptors(instance);
}

function getProblemDetails(error: AxiosError<ProblemDetails>): ProblemDetails {
	// If response already contains proper ProblemDetails, return it
	if (error.response && isProblemDetails(error.response)) {
		return ProblemDetailsUtils.applyDefaults(error.response.data, error.response.status);
	}

	// create a problem deatils if response is not a problem deatil
	return ProblemDetailsUtils.applyDefaults(
		{
			status: error.response?.status,
			detail: error.message,
			instance: error.config?.url,
		},
		error.response?.status,
	);
}

function showErrorToast(problemDetails: ProblemDetails): void {
	toast.error(problemDetails.title || 'An error occurred');
}

function isProblemDetails(response: AxiosResponse): boolean {
	const contentType = response.headers['content-type'];
	return (
		contentType?.includes('application/problem+json') ||
		(response.data && typeof response.data === 'object' && ('type' in response.data || 'title' in response.data))
	);
}
