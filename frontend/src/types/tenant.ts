export interface RegisterTenantRequest {
  countryCode: string;
  firstName: string;
  lastName: string;
  mobileNumber: string;
  email: string;
  pin: string;
  confirmPin: string;
}

export interface RegisterTenantResponse {
  tenantId: string;
  message: string;
}

export interface RegisterTenantFieldErrors {
  countryCode?: string;
  firstName?: string;
  lastName?: string;
  mobileNumber?: string;
  email?: string;
  pin?: string;
  confirmPin?: string;
}
