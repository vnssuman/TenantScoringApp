export interface TenantLookupResult {
  tenantId: string;
  profileId: string;
  fullName: string;
}

export interface RentalRequestRecord {
  leaseId: string;
  propertyId: string;
  propertyName: string;
  location: string;
  tenantId: string;
  profileId: string;
  tenantName: string;
  agreedRent: number;
  startDate: string | null;
  endDate: string | null;
  status: string;
  hasAgreementDocument: boolean;
}

export interface RentalRequestInput {
  propertyId: string;
  tenantId: string;
  rentAmount: number;
  startDate: string;
  endDate: string;
  agreementDocument?: File;
}