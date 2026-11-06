import type { ProfileStatus } from './auth';

export type ProfileRole = 'Owner' | 'Tenant' | 'Both';

export interface UserProfile {
  userId: string;
  profileId: string;
  createdAt: string;
  firstName?: string;
  lastName?: string;
  mobileNumber?: string;
  email?: string;
  panNumber?: string;
  aadhaarNumber?: string;
  mobileVerified: boolean;
  emailVerified: boolean;
  panVerified: boolean;
  aadhaarVerified: boolean;
  behavioralAssessmentCompleted: boolean;
  behavioralScore: number;
  profileCompletionScore: number;
  rewardPoints: number;
  currentRole: string;
  profileStatus: ProfileStatus;
}

export interface ProfileUpdateRequest {
  firstName: string;
  lastName: string;
  mobileNumber: string;
  email: string;
  role: ProfileRole;
  panNumber: string;
  aadhaarNumber: string;
}

export interface VerificationRequest {
  channel: 'mobile' | 'email' | 'pan' | 'aadhaar';
  code: string;
}

export interface BehavioralQuestion {
  questionId: string;
  category: string;
  questionText: string;
}

export interface BehavioralAssessment {
  completed: boolean;
  score: number;
  questions: BehavioralQuestion[];
  answers?: BehavioralAnswer[];
}

export interface BehavioralAnswer {
  questionId: string;
  answer: 'Yes' | 'No';
}