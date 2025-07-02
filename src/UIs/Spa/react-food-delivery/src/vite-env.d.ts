/// <reference types="vite/client" />

// Path alias declarations
declare module "@app/*";
declare module "@features/*";
declare module "@entities/*";
declare module "@shared/*";
declare module "@pages/*";

interface ImportMetaEnv {
  readonly VITE_GATEWAY_API_BASE_URL: string;
  readonly VITE_APP_ENV: "development" | "production" | "test";
  readonly VITE_APP_URL: string;
  readonly VITE_APP_NAME: string;
}

interface ImportMeta {
  readonly env: ImportMetaEnv;
}
