import type { RentalRequestInput, RentalRequestRecord, TenantLookupResult } from '../types/rentalRequest';

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL ?? '';

async function request<T>(path: string, options: RequestInit = {}): Promise<T> {
  const token = localStorage.getItem('kirayeeToken');
  const response = await fetch(`${API_BASE_URL}${path}`, {
    ...options,
    headers: { Authorization: `Bearer ${token ?? ''}`, ...options.headers },
  });
  if (!response.ok) {
    const payload = (await response.json().catch(() => null)) as { message?: string } | null;
    throw new Error(payload?.message ?? 'Rental requests could not be loaded or saved.');
  }
  return (await response.json()) as T;
}

export function getRentalRequests(status: 'Active' | 'Pending'): Promise<RentalRequestRecord[]> {
  return request<RentalRequestRecord[]>(`/api/rental-requests?status=${status}`);
}

export function searchTenants(query: string): Promise<TenantLookupResult[]> {
  return request<TenantLookupResult[]>(`/api/tenants/search?q=${encodeURIComponent(query)}`);
}

export function sendRentalRequest(input: RentalRequestInput): Promise<RentalRequestRecord> {
  const form = new FormData();
  form.set('propertyId', input.propertyId);
  form.set('tenantId', input.tenantId);
  form.set('rentAmount', String(input.rentAmount));
  form.set('startDate', input.startDate);
  form.set('endDate', input.endDate);
  if (input.agreementDocument) form.set('agreementDocument', input.agreementDocument);
  return request<RentalRequestRecord>('/api/rental-requests', { method: 'POST', body: form });
}

export async function downloadRentalAgreement(leaseId: string): Promise<void> {
  const token = localStorage.getItem('kirayeeToken');
  const response = await fetch(`${API_BASE_URL}/api/rental-requests/${encodeURIComponent(leaseId)}/document`, {
    headers: { Authorization: `Bearer ${token ?? ''}` },
  });
  if (!response.ok) throw new Error('Agreement document could not be opened.');
  const url = URL.createObjectURL(await response.blob());
  const link = document.createElement('a');
  link.href = url;
  link.download = 'rental-agreement.pdf';
  link.click();
  URL.revokeObjectURL(url);
}