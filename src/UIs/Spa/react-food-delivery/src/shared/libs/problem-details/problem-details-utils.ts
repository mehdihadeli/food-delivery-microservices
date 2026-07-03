import type { ProblemDetails } from './models/problem-details';
import { getDefaultProblemDetails } from './problem-details-defaults';

export class ProblemDetailsUtils {
	public static applyDefaults(problemDetails: ProblemDetails, statusCode?: number): ProblemDetails {
		// Set status code if not provided
		if (problemDetails.status === undefined || problemDetails.status === null) {
			problemDetails.status = statusCode ?? (this.isValidationProblem(problemDetails) ? 400 : 500);
		}

		const finalStatusCode = problemDetails.status;

		// Apply defaults if available
		const defaults = getDefaultProblemDetails(finalStatusCode);
		problemDetails.type = problemDetails.type ?? defaults.type;
		problemDetails.title = problemDetails.title ?? defaults.title;

		return problemDetails;
	}

	private static isValidationProblem(problem: ProblemDetails): boolean {
		// Check if it's a validation problem by looking for errors property
		return 'errors' in problem;
	}
}
