/// <reference types="vite/client" />

interface ImportMetaEnv {
  /** Optional absolute API URL, e.g. "http://localhost:5056". Empty = use the dev proxy. */
  readonly VITE_API_BASE_URL?: string;
}

interface ImportMeta {
  readonly env: ImportMetaEnv;
}
