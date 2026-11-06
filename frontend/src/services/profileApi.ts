import type { ProfileStatus } from '../types/auth';
import type { BehavioralAnswer, BehavioralAssessment, ProfileUpdateRequest, UserProfile, VerificationRequest } from '../types/profile';

async function request<T>(path: string, options: RequestInit = {}): Promise<T> {
  const token = localStorage.getItem('kirayeeToken');
  const response = await fetch(path, {
    ...options,
    headers: { 'Content-Type': 'application/json', Authorization: `Bearer ${token ?? ''}`, ...options.headers },
  });
  if (!response.ok) {
    const payload = (await response.json().catch(() => null)) as { message?: string } | null;
    throw new Error(payload?.message ?? 'Your profile could not be loaded or updated.');
  }
  return (await response.json()) as T;
}

export function getCurrentProfile(): Promise<UserProfile> { return request<UserProfile>('/api/user/me'); }
export function updateProfile(data: ProfileUpdateRequest): Promise<UserProfile> { return request<UserProfile>('/api/kirayee/profile', { method: 'PUT', body: JSON.stringify(data) }); }
export function verifyProfile(data: VerificationRequest): Promise<{ verified: boolean }> { return request<{ verified: boolean }>('/api/kirayee/verification', { method: 'POST', body: JSON.stringify(data) }); }
export function getBehavioralAssessment(): Promise<BehavioralAssessment> { return request<BehavioralAssessment>('/api/kirayee/behavioral-assessment'); }
export function submitBehavioralAssessment(answers: BehavioralAnswer[]): Promise<BehavioralAssessment> { return request<BehavioralAssessment>('/api/kirayee/behavioral-assessment', { method: 'POST', body: JSON.stringify({ answers }) }); }
export async function getProfileImage(): Promise<Blob | null> {
  const token = localStorage.getItem('kirayeeToken');
  const response = await fetch('/api/user/profile-image', { headers: { Authorization: `Bearer ${token ?? ''}` } });
  if (response.status === 404) return null;
  if (!response.ok) throw new Error('Profile image could not be loaded.');
  return response.blob();
}
export async function uploadProfileImage(image: File): Promise<void> {
  const form = new FormData();
  form.set('image', image);
  const token = localStorage.getItem('kirayeeToken');
  const response = await fetch('/api/user/profile-image', {
    method: 'POST',
    headers: { Authorization: `Bearer ${token ?? ''}` },
    body: form,
  });
  if (!response.ok) {
    const payload = (await response.json().catch(() => null)) as { message?: string } | null;
    throw new Error(payload?.message ?? 'Profile image could not be updated.');
  }
}
export function getProfileStatus(profile: UserProfile): ProfileStatus { return profile.profileStatus; }