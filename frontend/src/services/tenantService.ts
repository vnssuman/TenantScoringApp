import type { RegisterTenantRequest, RegisterTenantResponse } from '../types/tenant';

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL ?? '';

interface ApiErrorPayload {
  message?: string;
  errors?: string[];
}

export class TenantRegistrationError extends Error {
  constructor(message: string) {
    super(message);
    this.name = 'TenantRegistrationError';
  }
}

export async function registerTenant(
  request: RegisterTenantRequest,
): Promise<RegisterTenantResponse> {
  const response = await fetch(`${API_BASE_URL}/api/tenants/register`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(request),
  });

  if (!response.ok) {
    const payload = (await response.json().catch(() => null)) as ApiErrorPayload | null;
    const message = payload?.message ?? payload?.errors?.[0] ?? 'Registration could not be completed.';
    throw new TenantRegistrationError(message);
  }

  return (await response.json()) as RegisterTenantResponse;
}
