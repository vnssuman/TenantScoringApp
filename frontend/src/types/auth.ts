export interface LoginRequest {
  countryCode: string;
  mobileNumber: string;
  pin: string;
}

export interface LoginResponse {
  userId: string;
  firstName?: string;
  currentRole: string;
  token: string;
  profileStatus: ProfileStatus;
}

export interface ProfileStatus {
  isProfileComplete: boolean;
  completionPercentage: number;
  isMobileVerified: boolean;
  isEmailVerified: boolean;
  isPanVerified: boolean;
  isAadhaarVerified: boolean;
  isPanProvided: boolean;
  isAadhaarProvided: boolean;
  behavioralAssessmentCompleted: boolean;
  behavioralScore: number;
  missingFields: string[];
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
