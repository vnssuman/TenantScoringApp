import type { PropertyInput, PropertyRecord } from '../types/property';

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL ?? '';

async function request<T>(path: string, options: RequestInit = {}): Promise<T> {
  const token = localStorage.getItem('kirayeeToken');
  const response = await fetch(`${API_BASE_URL}${path}`, {
    ...options,
    headers: {
      'Content-Type': 'application/json',
      Authorization: `Bearer ${token ?? ''}`,
      ...options.headers,
    },
  });

  if (!response.ok) {
    const payload = (await response.json().catch(() => null)) as { message?: string } | null;
    throw new Error(payload?.message ?? 'Properties could not be loaded or saved.');
  }

  if (response.status === 204) return undefined as T;
  return (await response.json()) as T;
}

export function getProperties(userId: string): Promise<PropertyRecord[]> {
  return request<PropertyRecord[]>(`/api/properties?userId=${encodeURIComponent(userId)}`);
}

export function createProperty(property: PropertyInput): Promise<PropertyRecord> {
  return request<PropertyRecord>('/api/properties', { method: 'POST', body: JSON.stringify(property) });
}

export function updateProperty(id: string, property: PropertyInput): Promise<PropertyRecord> {
  return request<PropertyRecord>(`/api/properties/${encodeURIComponent(id)}`, { method: 'PUT', body: JSON.stringify(property) });
}