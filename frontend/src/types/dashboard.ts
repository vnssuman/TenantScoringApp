export type DashboardView = 'owner' | 'renter';

export interface DashboardUserSummary {
  tenantId: string;
  fullName: string;
  assignedRoles: string[];
  mobileNumber: string;
  email: string;
}

export interface DashboardAction {
  title: string;
  description: string;
  linkLabel: string;
  tone: 'green' | 'purple' | 'blue' | 'gold';
}
