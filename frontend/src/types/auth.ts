export interface LoginRequest {
  countryCode: string;
  mobileNumber: string;
  pin: string;
}

export interface LoginResponse {
  tenantId: string;
  firstName: string;
  token: string;
}

export interface ResetPinRequest {
  countryCode: string;
  mobileNumber: string;
  newPin: string;
  confirmNewPin: string;
}

export interface ResetPinResponse {
  message: string;
}

export interface AuthFieldErrors {
  countryCode?: string;
  mobileNumber?: string;
  pin?: string;
  newPin?: string;
  confirmNewPin?: string;
}
