import type { FeedbackHistoryItem, FeedbackPendingSession, FeedbackRating } from '../types/feedback';

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL ?? '';

async function request<T>(path: string, options: RequestInit = {}): Promise<T> {
  const token = localStorage.getItem('kirayeeToken');
  const response = await fetch(`${API_BASE_URL}${path}`, {
    ...options,
    headers: { Authorization: `Bearer ${token ?? ''}`, ...options.headers },
  });
  if (!response.ok) {
    const body = await response.text().catch(() => '');
    let message: string | undefined;
    try {
      message = (JSON.parse(body) as { message?: string } | null)?.message;
    } catch {
      message = undefined;
    }
    throw new Error(message || `Feedback request failed (HTTP ${response.status}).`);
  }
  return (await response.json()) as T;
}

export function getPendingFeedback(): Promise<FeedbackPendingSession[]> {
  return request<FeedbackPendingSession[]>('/api/feedback/pending');
}

export function getFeedbackHistory(): Promise<FeedbackHistoryItem[]> {
  return request<FeedbackHistoryItem[]>('/api/feedback/history');
}

export function submitFeedback(sessionId: string, responses: FeedbackRating[]): Promise<void> {
  return request<void>('/api/feedback/submit', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ sessionId, responses }),
  });
}
