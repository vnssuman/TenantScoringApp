export interface FeedbackQuestion {
  questionId: string;
  questionText: string;
  displayOrder: number;
}

export interface FeedbackPendingSession {
  sessionId: string;
  leaseId: string;
  propertyId: string;
  propertyName: string;
  subjectName: string;
  feedbackType: 'TENANT_TO_OWNER' | 'OWNER_TO_TENANT';
  questions: FeedbackQuestion[];
}

export interface FeedbackHistoryRating {
  questionText: string;
  displayOrder: number;
  rating: number;
}

export interface FeedbackHistoryItem {
  sessionId: string;
  leaseId: string;
  propertyId: string;
  propertyName: string;
  feedbackType: 'TENANT_TO_OWNER' | 'OWNER_TO_TENANT';
  direction: 'GIVEN' | 'RECEIVED';
  counterpartyName: string;
  overallScore: number;
  completedAt: string;
  responses: FeedbackHistoryRating[];
}

export interface FeedbackRating {
  questionId: string;
  rating: number;
}
