import type {
  LoginRequest,
  LoginResponse,
  ResetPinRequest,
  ResetPinResponse,
} from '../types/auth';

interface ApiErrorPayload {
  message?: string;
  errors?: string[];
}

export class AuthApiError extends Error {
  constructor(message: string) {
    super(message);
    this.name = 'AuthApiError';
  }
}

async function post<TResponse>(path: string, data: LoginRequest | ResetPinRequest): Promise<TResponse> {
  const response = await fetch(path, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(data),
  });

  if (!response.ok) {
    const payload = (await response.json().catch(() => null)) as ApiErrorPayload | null;
    throw new AuthApiError(payload?.message ?? payload?.errors?.[0] ?? 'The request could not be completed.');
  }

  return (await response.json()) as TResponse;
}

export function loginUser(data: LoginRequest): Promise<LoginResponse> {
  return post<LoginResponse>('/api/auth/login', data);
}

export function resetPin(data: ResetPinRequest): Promise<ResetPinResponse> {
  return post<ResetPinResponse>('/api/auth/reset-pin', data);
}
