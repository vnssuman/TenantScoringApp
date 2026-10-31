import type { ProfileStatus } from '../types/auth';
import type { BehavioralAnswer, BehavioralAssessment, ProfileUpdateRequest, UserProfile, VerificationRequest } from '../types/profile';

async function request<T>(path: string, options: RequestInit = {}): Promise<T> {
  const token = localStorage.getItem('kirayeeToken');
  const response = await fetch(path, {
    ...options,
    headers: { 'Content-Type': 'application/json', Authorization: `Bearer ${token ?? ''}`, ...options.headers },
  });
  if (!response.ok) throw new Error('Your profile could not be loaded or updated.');
  return (await response.json()) as T;
}

export function getCurrentProfile(): Promise<UserProfile> { return request<UserProfile>('/api/user/me'); }
export function updateProfile(data: ProfileUpdateRequest): Promise<UserProfile> { return request<UserProfile>('/api/kirayee/profile', { method: 'PUT', body: JSON.stringify(data) }); }
export function verifyProfile(data: VerificationRequest): Promise<{ verified: boolean }> { return request<{ verified: boolean }>('/api/kirayee/verification', { method: 'POST', body: JSON.stringify(data) }); }
export function getBehavioralAssessment(): Promise<BehavioralAssessment> { return request<BehavioralAssessment>('/api/kirayee/behavioral-assessment'); }
export function submitBehavioralAssessment(answers: BehavioralAnswer[]): Promise<BehavioralAssessment> { return request<BehavioralAssessment>('/api/kirayee/behavioral-assessment', { method: 'POST', body: JSON.stringify({ answers }) }); }
export function getProfileStatus(profile: UserProfile): ProfileStatus { return profile.profileStatus; }