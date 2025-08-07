import type { ProblemDetails } from "@shared/models/problem-details";
import type { UserClaim } from "@shared/models/user-claim";
import axios, { AxiosError, type AxiosInstance } from "axios";

const BFF_ENDPOINTS = {
  LOGIN: "/bff/login",
  SILENT_LOGIN: "/bff/silent-login",
  SILENT_LOGIN_CALLBACK: "/bff/silent-login-callback",
  LOGOUT: "/bff/logout",
  USER: "/bff/user",
  BACKCHANNEL_LOGOUT: "/bff/backchannel",
  DIAGNOSTICS: "/bff/diagnostics",
} as const;

interface AuthState {
  isAuthenticated: boolean;
  logoutUrl: string;
  userClaims: UserClaim[] | null;
  isLoading: boolean;
  diagnostics?: any;
  error: ProblemDetails | null;
}

class AuthService {
  private static instance: AuthService;
  private axiosInstance: AxiosInstance;
  private state: AuthState;
  private subscribers: Array<(state: AuthState) => void> = [];
  private apiBaseAddress: string;

  private constructor() {
    this.apiBaseAddress = "/gateway/spa-bff"
    this.axiosInstance = axios.create({
      baseURL: this.apiBaseAddress,
      withCredentials: true,
      headers: {
        "X-CSRF": "1",
        "Content-Type": "application/json",
      },
    });

    this.state = {
      isAuthenticated: false,
      logoutUrl: `${this.apiBaseAddress}${BFF_ENDPOINTS.LOGOUT}`,
      userClaims: null,
      isLoading: false,
      error: null,
    };

    // Add response interceptor for ProblemDetails
    this.axiosInstance.interceptors.response.use(
        (response) => response,
        (error: AxiosError<ProblemDetails>) => {
          if (
              error.response?.headers["content-type"]?.includes(
                  "application/problem+json"
              )
          ) {
            return Promise.reject(error);
          }

          // Convert to ProblemDetails if not already
          const problemDetails: ProblemDetails = {
            type: `https://httpstatuses.com/${error.response?.status || 500}`,
            title: error.response?.statusText || "Internal Server Error",
            status: error.response?.status,
            detail: error.message,
          };

          return Promise.reject({
            ...error,
            response: {
              ...error.response,
              data: problemDetails,
            },
          });
        }
    );
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
      const response = await this.axiosInstance.get<UserClaim[]>(
          BFF_ENDPOINTS.USER
      );
      const claims = response.data;

      // Update service state
      let logoutUrl;
      var claimLogout = claims.find(
          (claim) => claim.type === "bff:logout_url"
      )?.value;
      if (claimLogout) {
        logoutUrl = `${this.apiBaseAddress}${claimLogout}`;
      } else {
        logoutUrl = this.state.logoutUrl;
      }

      this.setState({
        isAuthenticated: true,
        logoutUrl,
        userClaims: claims,
        isLoading: false,
      });

      return claims;
    } catch (error) {
      this.handleAuthError(error);
      this.setState({
        isAuthenticated: false,
        userClaims: null,
      });
      console.log(error);
      return null;
    }
  }

  public async getDiagnostics(): Promise<any> {
    try {
      const response = await this.axiosInstance.get(BFF_ENDPOINTS.DIAGNOSTICS);
      this.setState({ diagnostics: response.data });
      return response.data;
    } catch (error) {
      this.handleAuthError(error);
      throw error;
    }
  }

  public login(returnUrl: string = window.location.pathname): void {
    window.location.href = `${this.apiBaseAddress}${
        BFF_ENDPOINTS.LOGIN
    }?returnUrl=${encodeURIComponent(returnUrl)}`;
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
    const baseUrl = this.state.logoutUrl.includes("?")
        ? `${this.state.logoutUrl}&`
        : `${this.state.logoutUrl}?`;

    return `${baseUrl}returnUrl=${encodeURIComponent(postLogoutRedirectUri)}`;
  }

  private setState(partialState: Partial<AuthState>): void {
    this.state = { ...this.state, ...partialState };
    this.notifySubscribers();
  }

  private handleAuthError(error: unknown): void {
    let problemDetails: ProblemDetails = {
      type: "about:blank",
      title: "Authentication Error",
      status: 500,
      detail: "An unexpected authentication error occurred",
    };

    if (axios.isAxiosError<ProblemDetails>(error)) {
      // Use ProblemDetails from response if available
      if (error.response?.data) {
        problemDetails = {
          ...problemDetails,
          ...error.response.data,
          status: error.response.status || problemDetails.status,
        };
      } else {
        // Fallback to HTTP status based details
        problemDetails = {
          ...problemDetails,
          type: `https://httpstatuses.com/${error.response?.status || 500}`,
          title: error.response?.statusText || "Request Failed",
          status: error.response?.status,
          detail: error.message,
        };
      }
    } else if (error instanceof Error) {
      problemDetails = {
        ...problemDetails,
        detail: error.message,
      };
    }

    this.setState({
      isAuthenticated: false,
      logoutUrl: BFF_ENDPOINTS.LOGOUT,
      userClaims: null,
      isLoading: false,
      error: problemDetails,
    });
  }
}

export const authService = AuthService.getInstance();
