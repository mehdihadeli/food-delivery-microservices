import type { UserClaim } from '@shared/models/user-claim';
import { ApiClient } from './api-client';
import type { ProblemDetails } from '@shared/libs/problem-details/models/problem-details';

interface AuthState {
	isAuthenticated: boolean;
	logoutUrl: string;
	userClaims: UserClaim[] | null;
	isLoading: boolean;
	diagnostics?: any;
	error: ProblemDetails | null;
}

const BFF_ENDPOINTS = {
	LOGIN: '/bff/login',
	SILENT_LOGIN: '/bff/silent-login',
	SILENT_LOGIN_CALLBACK: '/bff/silent-login-callback',
	LOGOUT: '/bff/logout',
	USER: '/bff/user',
	BACKCHANNEL_LOGOUT: '/bff/backchannel',
	DIAGNOSTICS: '/bff/diagnostics',
} as const;

class AuthService extends ApiClient {
	private static instance: AuthService;
	private state: AuthState;
	private subscribers: Array<(state: AuthState) => void> = [];

	private constructor() {
		super('/gateway/spa-bff');

		this.state = {
			isAuthenticated: false,
			logoutUrl: `${this.baseUrl}${BFF_ENDPOINTS.LOGOUT}`,
			userClaims: null,
			isLoading: false,
			error: null,
		};
	}

	public static getInstance(): AuthService {
		if (!AuthService.instance) {
			AuthService.instance = new AuthService();
		}
		return AuthService.instance;
	}

	private notifySubscribers(): void {
		this.subscribers.forEach((callback) => callback(this.state));
	}

	public subscribe(callback: (state: AuthState) => void): () => void {
		this.subscribers.push(callback);
		callback(this.state);
		return () => {
			this.subscribers = this.subscribers.filter((cb) => cb !== callback);
		};
	}

	public get currentState(): AuthState {
		return this.state;
	}

	public async getUserClaims(): Promise<UserClaim[] | null> {
		this.setState({ isLoading: true, error: null });

		try {
			const claims = await this.get<UserClaim[]>(BFF_ENDPOINTS.USER);

			// Update service state
			const logoutUrl = claims.find((claim) => claim.type === 'bff:logout_url')?.value
				? `${this.baseUrl}${claims.find((claim) => claim.type === 'bff:logout_url')!.value}`
				: this.state.logoutUrl;

			this.setState({
				isAuthenticated: true,
				logoutUrl,
				userClaims: claims,
				isLoading: false,
			});

			return claims;
		} catch {
			this.setState({
				isAuthenticated: false,
				userClaims: null,
				isLoading: false,
			});
			return null;
		}
	}

	public async getDiagnostics(): Promise<any> {
		const diagnostics = await this.get(BFF_ENDPOINTS.DIAGNOSTICS);
		this.setState({ diagnostics });
		return diagnostics;
	}

	public login(returnUrl: string = window.location.pathname): void {
		window.location.href = `${this.baseUrl}${BFF_ENDPOINTS.LOGIN}?returnUrl=${encodeURIComponent(returnUrl)}`;
	}

	public logout(postLogoutRedirectUri: string = window.location.origin): void {
		const logoutUrl = this.buildLogoutUrl(postLogoutRedirectUri);

		// Clear auth state immediately
		this.setState({
			isAuthenticated: false,
			userClaims: null,
		});

		// Redirect to logout endpoint
		window.location.href = logoutUrl;
	}

	private buildLogoutUrl(postLogoutRedirectUri: string): string {
		const separator = this.state.logoutUrl.includes('?') ? '&' : '?';
		return `${this.state.logoutUrl}${separator}returnUrl=${encodeURIComponent(postLogoutRedirectUri)}`;
	}

	private setState(partialState: Partial<AuthState>): void {
		this.state = { ...this.state, ...partialState };
		this.notifySubscribers();
	}

	private get baseUrl(): string {
		return this.api.defaults.baseURL || '';
	}
}

export const authService = AuthService.getInstance();
