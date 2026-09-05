import type { DashboardUserSummary } from '../types/dashboard';

export async function getDashboardSummary(token: string): Promise<DashboardUserSummary> {
  const response = await fetch('/api/user/dashboard-summary', {
    headers: { Authorization: `Bearer ${token}` },
  });

  if (!response.ok) {
    throw new Error('Your session has expired. Please log in again.');
  }

  return (await response.json()) as DashboardUserSummary;
}
