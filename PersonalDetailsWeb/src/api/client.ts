import type { ProblemDetails } from './types';

/**
 * Base URL of the API. Empty by default, so requests go to the same origin ("/api/...")
 * and the Vite dev server proxies them to the ASP.NET Core app (see vite.config.ts).
 * Set VITE_API_BASE_URL (e.g. in .env.local) to call the API directly instead; the API
 * must then allow this origin in its "Cors:AllowedOrigins" setting.
 */
const BASE_URL = (import.meta.env.VITE_API_BASE_URL ?? '').replace(/\/$/, '');

/** Thrown for any non-2xx response. Carries the ProblemDetails body sent by the API. */
export class ApiError extends Error {
  readonly status: number;
  readonly problem: ProblemDetails;

  constructor(status: number, problem: ProblemDetails) {
    super(problem.detail ?? problem.title ?? `Request failed with status ${status}`);
    this.name = 'ApiError';
    this.status = status;
    this.problem = problem;
  }

  /** Field-level validation messages (400 responses), keyed by field path, e.g. "Addresses[0].City". */
  get fieldErrors(): Record<string, string[]> {
    return this.problem.errors ?? {};
  }
}

type QueryValue = string | number | boolean | null | undefined;

/** Builds "?a=1&b=2", skipping empty values so the API applies its own defaults. */
export function toQueryString(params: Record<string, QueryValue>): string {
  const search = new URLSearchParams();
  for (const [key, value] of Object.entries(params)) {
    if (value === undefined || value === null || value === '') continue;
    search.set(key, String(value));
  }
  const text = search.toString();
  return text ? `?${text}` : '';
}

/**
 * Small wrapper around fetch: sends/receives JSON, turns error responses into ApiError,
 * and accepts an AbortSignal (the browser's equivalent of a .NET CancellationToken).
 */
export async function request<T>(path: string, options: { method?: string; body?: unknown; signal?: AbortSignal } = {}): Promise<T> {
  const response = await fetch(`${BASE_URL}${path}`, {
    method: options.method ?? 'GET',
    headers: {
      Accept: 'application/json',
      ...(options.body !== undefined ? { 'Content-Type': 'application/json' } : {}),
    },
    body: options.body !== undefined ? JSON.stringify(options.body) : undefined,
    signal: options.signal,
  });

  if (!response.ok) {
    let problem: ProblemDetails = { status: response.status, title: response.statusText };
    try {
      problem = await response.json();
    } catch {
      // Body was empty or not JSON (e.g. the proxy could not reach the API); keep the fallback.
    }
    throw new ApiError(response.status, problem);
  }

  // 204 No Content (DELETE) has no body.
  if (response.status === 204) return undefined as T;
  return (await response.json()) as T;
}
