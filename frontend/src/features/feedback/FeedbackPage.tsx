import { useEffect, useState, type ReactElement } from 'react';
import { ArrowLeft, CheckCircle2, ChevronDown, Star } from 'lucide-react';
import { getFeedbackHistory, getPendingFeedback, submitFeedback } from '../../services/feedbackApi';
import type { FeedbackHistoryItem, FeedbackPendingSession } from '../../types/feedback';

type Ratings = Record<string, Record<string, number>>;
type FeedbackSection = 'pending' | 'history';

export function FeedbackPage({ onBack, onPendingCountChanged }: { onBack: () => void; onPendingCountChanged: (count: number) => void }): ReactElement {
  const [sessions, setSessions] = useState<FeedbackPendingSession[]>([]);
  const [history, setHistory] = useState<FeedbackHistoryItem[]>([]);
  const [expandedHistory, setExpandedHistory] = useState<Record<string, boolean>>({});
  const [section, setSection] = useState<FeedbackSection>('pending');
  const [ratings, setRatings] = useState<Ratings>({});
  const [loading, setLoading] = useState(true);
  const [submittingSession, setSubmittingSession] = useState('');
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');

  async function loadFeedback(): Promise<void> {
    setError('');
    try {
      const [pending, completed] = await Promise.all([getPendingFeedback(), getFeedbackHistory()]);
      setSessions(pending);
      setHistory(completed);
      onPendingCountChanged(pending.length);
    } catch (reason) {
      setError(reason instanceof Error ? reason.message : 'Feedback requests could not be loaded.');
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    void loadFeedback();
  }, []);

  function setRating(sessionId: string, questionId: string, rating: number): void {
    setRatings((current) => ({
      ...current,
      [sessionId]: { ...current[sessionId], [questionId]: rating },
    }));
    setSuccess('');
  }

  async function submit(session: FeedbackPendingSession): Promise<void> {
    const responses = session.questions.map((question) => ({
      questionId: question.questionId,
      rating: ratings[session.sessionId]?.[question.questionId] ?? 0,
    }));
    if (responses.some((response) => response.rating < 1 || response.rating > 5)) {
      setError('Please rate all 10 questions before submitting.');
      return;
    }
    setError('');
    setSubmittingSession(session.sessionId);
    try {
      await submitFeedback(session.sessionId, responses);
      const remaining = sessions.filter((item) => item.sessionId !== session.sessionId);
      setSessions(remaining);
      onPendingCountChanged(remaining.length);
      setSuccess('Your feedback has been submitted. Thank you.');
      void getFeedbackHistory()
        .then(setHistory)
        .catch((reason: unknown) => setError(reason instanceof Error ? reason.message : 'Feedback history could not be refreshed.'));
    } catch (reason) {
      setError(reason instanceof Error ? reason.message : 'Your feedback could not be submitted.');
    } finally {
      setSubmittingSession('');
    }
  }

  return (
    <main className="mobile-content feedback-page">
      <button className="properties-back" type="button" onClick={onBack}><ArrowLeft size={16} /> Services</button>
      <header className="properties-header">
        <div>
          <p className="mobile-kicker">Move-out feedback</p>
          <h1>Your Feedback</h1>
        </div>
      </header>
      <p className="feedback-intro">Rate each item from 1 to 5 stars. Your feedback requests are available after a tenancy ends.</p>
      <div className="feedback-tabs" role="tablist" aria-label="Feedback sections">
        <button type="button" role="tab" aria-selected={section === 'pending'} className={section === 'pending' ? 'active' : ''} onClick={() => setSection('pending')}>
          To complete <span>{sessions.length}</span>
        </button>
        <button type="button" role="tab" aria-selected={section === 'history'} className={section === 'history' ? 'active' : ''} onClick={() => setSection('history')}>
          Feedback history <span>{history.length}</span>
        </button>
      </div>
      {error && <p className="properties-error" role="alert">{error}</p>}
      {success && <p className="feedback-success" role="status"><CheckCircle2 size={16} />{success}</p>}
      {loading ? <p className="mapping-loading">Loading feedback...</p> : section === 'pending' ? sessions.length === 0 ? (
        <section className="mapping-empty-requests">
          <CheckCircle2 size={30} />
          <h2>All caught up</h2>
          <p>There are no pending feedback requests.</p>
        </section>
      ) : (
        <section className="feedback-sessions" aria-label="Pending feedback requests">
          {sessions.map((session) => (
            <article className="feedback-session-card" key={session.sessionId}>
              <header>
                <p className="mobile-kicker">{session.feedbackType === 'TENANT_TO_OWNER' ? 'Tenant feedback for owner' : 'Owner feedback for tenant'}</p>
                <h2>{session.propertyName}</h2>
                <p>{session.feedbackType === 'TENANT_TO_OWNER' ? 'Rate the owner' : 'Rate the tenant'}: <strong>{session.subjectName || 'Former rental partner'}</strong></p>
              </header>
              <ol className="feedback-question-list">
                {session.questions.map((question) => {
                  const rating = ratings[session.sessionId]?.[question.questionId] ?? 0;
                  return (
                    <li key={question.questionId}>
                      <p>{question.questionText}</p>
                      <div className="feedback-star-rating" role="group" aria-label={`Rating for question ${question.displayOrder}`}>
                        {[1, 2, 3, 4, 5].map((value) => (
                          <button
                            key={value}
                            type="button"
                            aria-label={`${value} ${value === 1 ? 'star' : 'stars'}`}
                            aria-pressed={rating === value}
                            className={value <= rating ? 'selected' : ''}
                            onClick={() => setRating(session.sessionId, question.questionId, value)}
                          >
                            <Star size={20} fill={value <= rating ? 'currentColor' : 'none'} />
                          </button>
                        ))}
                        <span>{rating ? `${rating} / 5` : 'Required'}</span>
                      </div>
                    </li>
                  );
                })}
              </ol>
              <button
                className="mapping-primary-button feedback-submit"
                type="button"
                disabled={submittingSession === session.sessionId || session.questions.some((question) => !ratings[session.sessionId]?.[question.questionId])}
                onClick={() => void submit(session)}
              >
                {submittingSession === session.sessionId ? 'Submitting...' : 'Submit Feedback'}
              </button>
            </article>
          ))}
        </section>
      ) : history.length === 0 ? (
        <section className="mapping-empty-requests">
          <CheckCircle2 size={30} />
          <h2>No feedback yet</h2>
          <p>Feedback you receive from former rental partners will appear here.</p>
        </section>
      ) : (
        <section className="feedback-sessions" aria-label="Completed feedback history">
          {history.map((item) => (
            <article className="feedback-session-card feedback-history-card" key={item.sessionId}>
              <header>
                <p className="mobile-kicker">Feedback you received · {item.feedbackType === 'TENANT_TO_OWNER' ? 'Tenant to owner' : 'Owner to tenant'}</p>
                <span className="feedback-history-label">Property Name</span>
                <h2>{item.propertyName}</h2>
              </header>
              <div className="feedback-history-summary">
                <div>
                  <span>Rated By</span>
                  <strong>{item.counterpartyName || 'Former rental partner'}</strong>
                </div>
                <div>
                  <span>Overall Feedback</span>
                  <strong><Star size={15} fill="currentColor" /> {item.overallScore.toFixed(2)} / 5</strong>
                </div>
                <div>
                  <span>Date</span>
                  <time dateTime={item.completedAt}>{new Date(item.completedAt).toLocaleDateString()}</time>
                </div>
              </div>
              <button
                className="feedback-history-toggle"
                type="button"
                aria-expanded={expandedHistory[item.sessionId] ?? false}
                aria-controls={`feedback-details-${item.sessionId}`}
                onClick={() => setExpandedHistory((current) => ({ ...current, [item.sessionId]: !current[item.sessionId] }))}
              >
                {expandedHistory[item.sessionId] ? 'Hide full feedback' : 'View full feedback'}
                <ChevronDown size={16} aria-hidden="true" />
              </button>
              {expandedHistory[item.sessionId] && (
                <ol className="feedback-question-list" id={`feedback-details-${item.sessionId}`}>
                  {item.responses.map((response) => (
                    <li key={`${item.sessionId}-${response.displayOrder}`}>
                      <p>{response.questionText}</p>
                      <div className="feedback-history-rating" aria-label={`${response.rating} out of 5 stars`}>
                        {Array.from({ length: 5 }, (_, index) => <Star key={index} size={16} fill={index < response.rating ? 'currentColor' : 'none'} />)}
                        <span>{response.rating} / 5</span>
                      </div>
                    </li>
                  ))}
                </ol>
              )}
            </article>
          ))}
        </section>
      )}
    </main>
  );
}
