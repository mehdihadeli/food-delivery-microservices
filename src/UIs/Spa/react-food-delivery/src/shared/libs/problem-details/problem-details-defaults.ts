type ProblemDetailsDefaultsType = {
	type: string;
	title: string;
};

const ProblemDetailsDefaults: Record<number, ProblemDetailsDefaultsType> = {
	400: {
		type: 'https://tools.ietf.org/html/rfc9110#section-15.5.1',
		title: 'Bad Request',
	},
	401: {
		type: 'https://tools.ietf.org/html/rfc9110#section-15.5.2',
		title: 'Unauthorized',
	},
	403: {
		type: 'https://tools.ietf.org/html/rfc9110#section-15.5.4',
		title: 'Forbidden',
	},
	404: {
		type: 'https://tools.ietf.org/html/rfc9110#section-15.5.5',
		title: 'Not Found',
	},
	405: {
		type: 'https://tools.ietf.org/html/rfc9110#section-15.5.6',
		title: 'Method Not Allowed',
	},
	406: {
		type: 'https://tools.ietf.org/html/rfc9110#section-15.5.7',
		title: 'Not Acceptable',
	},
	407: {
		type: 'https://tools.ietf.org/html/rfc9110#section-15.5.8',
		title: 'Proxy Authentication Required',
	},
	408: {
		type: 'https://tools.ietf.org/html/rfc9110#section-15.5.9',
		title: 'Request Timeout',
	},
	409: {
		type: 'https://tools.ietf.org/html/rfc9110#section-15.5.10',
		title: 'Conflict',
	},
	410: {
		type: 'https://tools.ietf.org/html/rfc9110#section-15.5.11',
		title: 'Gone',
	},
	411: {
		type: 'https://tools.ietf.org/html/rfc9110#section-15.5.12',
		title: 'Length Required',
	},
	412: {
		type: 'https://tools.ietf.org/html/rfc9110#section-15.5.13',
		title: 'Precondition Failed',
	},
	413: {
		type: 'https://tools.ietf.org/html/rfc9110#section-15.5.14',
		title: 'Content Too Large',
	},
	414: {
		type: 'https://tools.ietf.org/html/rfc9110#section-15.5.15',
		title: 'URI Too Long',
	},
	415: {
		type: 'https://tools.ietf.org/html/rfc9110#section-15.5.16',
		title: 'Unsupported Media Type',
	},
	416: {
		type: 'https://tools.ietf.org/html/rfc9110#section-15.5.17',
		title: 'Range Not Satisfiable',
	},
	417: {
		type: 'https://tools.ietf.org/html/rfc9110#section-15.5.18',
		title: 'Expectation Failed',
	},
	421: {
		type: 'https://tools.ietf.org/html/rfc9110#section-15.5.20',
		title: 'Misdirected Request',
	},
	422: {
		type: 'https://tools.ietf.org/html/rfc4918#section-11.2',
		title: 'Unprocessable Entity',
	},
	426: {
		type: 'https://tools.ietf.org/html/rfc9110#section-15.5.22',
		title: 'Upgrade Required',
	},
	500: {
		type: 'https://tools.ietf.org/html/rfc9110#section-15.6.1',
		title: 'An error occurred while processing your request.',
	},
	501: {
		type: 'https://tools.ietf.org/html/rfc9110#section-15.6.2',
		title: 'Not Implemented',
	},
	502: {
		type: 'https://tools.ietf.org/html/rfc9110#section-15.6.3',
		title: 'Bad Gateway',
	},
	503: {
		type: 'https://tools.ietf.org/html/rfc9110#section-15.6.4',
		title: 'Service Unavailable',
	},
	504: {
		type: 'https://tools.ietf.org/html/rfc9110#section-15.6.5',
		title: 'Gateway Timeout',
	},
	505: {
		type: 'https://tools.ietf.org/html/rfc9110#section-15.6.6',
		title: 'HTTP Version Not Supported',
	},
};
export function getDefaultProblemDetails(statusCode: number): ProblemDetailsDefaultsType {
	const details = ProblemDetailsDefaults[statusCode];
	if (!details) {
		throw new Error(`No default ProblemDetails found for status code ${statusCode}`);
	}
	return details;
}
