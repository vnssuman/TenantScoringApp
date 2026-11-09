import type { RentalAgreementTerms, RentalRequestInput, RentalRequestRecord, TenantLookupResult, TenantPaymentPropertyRecord, TenantRentalRequestRecord } from '../types/rentalRequest';

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL ?? '';

async function request<T>(path: string, options: RequestInit = {}): Promise<T> {
  const token = localStorage.getItem('kirayeeToken');
  const response = await fetch(`${API_BASE_URL}${path}`, {
    ...options,
    headers: { Authorization: `Bearer ${token ?? ''}`, ...options.headers },
  });
  if (!response.ok) {
    const body = await response.text().catch(() => '');
    let message: string | undefined;
    try {
      message = (JSON.parse(body) as { message?: string } | null)?.message;
    } catch {
      message = undefined;
    }
    if (!message) {
      message = response.status === 401
        ? 'Your session has expired. Please sign in again.'
        : response.status === 403
          ? 'You are not allowed to perform this request action.'
          : response.status === 404
            ? 'The rental request action endpoint was not found. Restart the API and try again.'
            : `Rental request action failed (HTTP ${response.status}).`;
    }
    throw new Error(message);
  }
  return (await response.json()) as T;
}

export function getRentalRequests(status?: 'Active' | 'Pending'): Promise<RentalRequestRecord[]> {
  const query = status ? `?status=${status}` : '';
  return request<RentalRequestRecord[]>(`/api/rental-requests${query}`);
}

export function getTenantRentalRequests(): Promise<TenantRentalRequestRecord[]> {
  return request<TenantRentalRequestRecord[]>('/api/tenant/rental-requests');
}

export function getTenantRentalHistory(): Promise<TenantRentalRequestRecord[]> {
  return request<TenantRentalRequestRecord[]>('/api/tenant/rental-history');
}

export function respondToRentalRequest(leaseId: string, action: 'accept' | 'request-cancellation' | 'accept-cancellation'): Promise<void> {
  return request<void>(`/api/kirayee/leases/${encodeURIComponent(leaseId)}/${action}`, { method: 'POST' });
}

export function sendBackRentalRequest(leaseId: string, comments: string): Promise<void> {
  return request<void>(`/api/kirayee/leases/${encodeURIComponent(leaseId)}/send-back`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ comments }),
  });
}

export function getTenantPaymentRecords(): Promise<TenantPaymentPropertyRecord[]> {
  return request<TenantPaymentPropertyRecord[]>('/api/tenant/payment-records');
}

export function payTenantRent(paymentId: string, amount: number, transactionReference: string): Promise<void> {
  return request<void>(`/api/kirayee/payments/${encodeURIComponent(paymentId)}/pay`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ amount, transactionReference }),
  });
}

export function searchTenants(query: string): Promise<TenantLookupResult[]> {
  return request<TenantLookupResult[]>(`/api/tenants/search?q=${encodeURIComponent(query)}`);
}

export function sendRentalRequest(input: RentalRequestInput): Promise<RentalRequestRecord> {
  return request<RentalRequestRecord>('/api/leases/send-request', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(input),
  });
}

export function withdrawRentalRequest(leaseId: string): Promise<void> {
  return request<void>(`/api/kirayee/leases/${encodeURIComponent(leaseId)}/withdraw`, { method: 'POST' });
}

export function updateSentBackRentalRequest(leaseId: string, input: RentalAgreementTerms): Promise<RentalRequestRecord> {
  return request<RentalRequestRecord>(`/api/rental-requests/${encodeURIComponent(leaseId)}/resubmit`, {
    method: 'PUT',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(input),
  });
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