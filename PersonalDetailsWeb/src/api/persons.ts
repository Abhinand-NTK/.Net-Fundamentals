import { request, toQueryString } from './client';
import type {
  PagedResult,
  PersonDetailResponse,
  PersonQueryParameters,
  PersonRequest,
  PersonStatisticsResponse,
  PersonSummaryResponse,
} from './types';

// One function per endpoint in PersonsController (api/v1/persons).
const ROOT = '/api/v1/persons';

export const personsApi = {
  search: (query: PersonQueryParameters, signal?: AbortSignal) =>
    request<PagedResult<PersonSummaryResponse>>(`${ROOT}${toQueryString({ ...query })}`, { signal }),

  get: (id: string, signal?: AbortSignal) =>
    request<PersonDetailResponse>(`${ROOT}/${id}`, { signal }),

  create: (body: PersonRequest) =>
    request<PersonDetailResponse>(ROOT, { method: 'POST', body }),

  update: (id: string, body: PersonRequest) =>
    request<PersonDetailResponse>(`${ROOT}/${id}`, { method: 'PUT', body }),

  remove: (id: string) =>
    request<void>(`${ROOT}/${id}`, { method: 'DELETE' }),

  statistics: (signal?: AbortSignal) =>
    request<PersonStatisticsResponse>(`${ROOT}/statistics`, { signal }),
};
