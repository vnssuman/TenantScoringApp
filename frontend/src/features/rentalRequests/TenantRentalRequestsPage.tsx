import { useEffect, useState, type ReactElement } from 'react';
import { ArrowLeft, CalendarDays, Check, CornerUpLeft, FileText, MapPin } from 'lucide-react';
import { ActionModal } from '../../components/ActionModal';
import { downloadRentalAgreement, getTenantRentalRequests, respondToRentalRequest, sendBackRentalRequest } from '../../services/rentalRequestApi';
import type { TenantRentalRequestRecord } from '../../types/rentalRequest';

type RequestAction = 'accept' | 'send-back';
type RequestTab = 'current' | 'history';

export function TenantRentalRequestsPage({ onBack }: { onBack: () => void }): ReactElement {
  const [requests, setRequests] = useState<TenantRentalRequestRecord[]>([]);
  const [activeTab, setActiveTab] = useState<RequestTab>('current');
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [selectedAction, setSelectedAction] = useState<{ request: TenantRentalRequestRecord; action: RequestAction } | null>(null);
  const [comments, setComments] = useState('');
  const [submitting, setSubmitting] = useState(false);

  async function loadRequests(): Promise<void> {
    setError('');
    try {
      setRequests(await getTenantRentalRequests());
    } catch (reason) {
      setError(reason instanceof Error ? reason.message : 'Rental requests could not be loaded.');
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    void loadRequests();
  }, []);

  async function respond(): Promise<void> {
    if (!selectedAction) return;
    setSubmitting(true);
    setError('');
    try {
      if (selectedAction.action === 'send-back') {
        await sendBackRentalRequest(selectedAction.request.leaseId, comments.trim());
      } else {
        await respondToRentalRequest(selectedAction.request.leaseId, 'accept');
      }
      await loadRequests();
      setSelectedAction(null);
      setComments('');
    } catch (reason) {
      setError(reason instanceof Error ? reason.message : 'Your response could not be saved.');
    } finally {
      setSubmitting(false);
    }
  }

  const isAccepting = selectedAction?.action === 'accept';
  const canSendBack = comments.trim().length > 0 && comments.trim().length <= 2000;
  const currentRequests = requests.filter((request) => request.status === 'Pending');
  const requestHistory = requests.filter((request) => request.status !== 'Pending');
  const visibleRequests = activeTab === 'current' ? currentRequests : requestHistory;

  return (
    <main className="mobile-content tenant-requests-page">
      <button className="properties-back" type="button" onClick={onBack}><ArrowLeft size={16} /> Services</button>
      <header className="properties-header">
        <div>
          <p className="mobile-kicker">Tenant workspace</p>
          <h1>Rental Requests</h1>
        </div>
      </header>
      {error && <p className="properties-error" role="alert">{error}</p>}
      {loading ? <p className="mapping-loading">Loading rental requests...</p> : (
        <>
          <div className="mapping-tabs" role="tablist" aria-label="Rental request views">
            <button className={activeTab === 'current' ? 'active' : ''} type="button" role="tab" aria-selected={activeTab === 'current'} onClick={() => setActiveTab('current')}>Current Requests ({currentRequests.length})</button>
            <button className={activeTab === 'history' ? 'active' : ''} type="button" role="tab" aria-selected={activeTab === 'history'} onClick={() => setActiveTab('history')}>Request History ({requestHistory.length})</button>
          </div>
          {visibleRequests.length === 0 ? (
            <section className="mapping-empty-requests">
              <FileText size={30} />
              <h2>{activeTab === 'current' ? 'No Current Requests' : 'No Request History'}</h2>
              <p>{activeTab === 'current' ? 'New requests from property owners will appear here.' : 'Requests you have responded to will appear here.'}</p>
            </section>
          ) : (
            <section className="mapping-list" aria-label={activeTab === 'current' ? 'Current rental requests' : 'Rental request history'}>
              {visibleRequests.map((request) => (
                <article className="tenant-rental-request-card" key={request.leaseId}>
                  <header className="rental-request-heading">
                    <div>
                      <h2>{request.propertyName}</h2>
                      <p><MapPin size={14} />{[request.address, request.city, request.state].filter(Boolean).join(', ')}</p>
                    </div>
                    <span className={`rental-request-status ${request.status.toLowerCase()}`}>{request.status === 'SentBack' ? 'Sent Back' : request.status}</span>
                  </header>
                  <dl className="tenant-request-details">
                    <div><dt>Owner</dt><dd>{request.ownerName || 'Property owner'}</dd></div>
                    <div><dt>Property type</dt><dd>{request.propertyType}</dd></div>
                    <div><dt>Monthly rent</dt><dd>{formatCurrency(request.agreedRent)}</dd></div>
                    <div><dt>Advance</dt><dd>{formatCurrency(request.agreedAdvance)}</dd></div>
                    <div><dt>Requested dates</dt><dd><CalendarDays size={14} />{formatDate(request.startDate)} - {formatDate(request.endDate)}</dd></div>
                    <div><dt>Request sent</dt><dd>{formatDate(request.createdAt)}</dd></div>
                  </dl>
                  {request.tenantComments && <p className="rental-request-comments"><strong>Your comments:</strong> {request.tenantComments}</p>}
                  <footer className="tenant-request-actions">
                    {request.hasAgreementDocument && <button className="tenant-request-document" type="button" onClick={() => { void downloadRentalAgreement(request.leaseId).catch((reason: unknown) => setError(reason instanceof Error ? reason.message : 'Agreement document could not be opened.')); }}><FileText size={15} /> Download agreement PDF</button>}
                    {request.status === 'Pending' && <div className="tenant-request-response-actions">
                      <button className="mapping-secondary-button" type="button" onClick={() => { setComments(''); setSelectedAction({ request, action: 'send-back' }); }}><CornerUpLeft size={15} /> Send back</button>
                      <button className="mapping-primary-button" type="button" onClick={() => setSelectedAction({ request, action: 'accept' })}><Check size={15} /> Accept</button>
                    </div>}
                  </footer>
                </article>
              ))}
            </section>
          )}
        </>
      )}
      <ActionModal
        isOpen={selectedAction !== null}
        title={isAccepting ? 'Accept rental request?' : 'Send request back?'}
        message={selectedAction ? <>{isAccepting ? <>Accept the request for <strong>{selectedAction.request.propertyName}</strong> from {selectedAction.request.ownerName || 'the owner'}?</> : <><p>Tell {selectedAction.request.ownerName || 'the owner'} what needs to be changed for <strong>{selectedAction.request.propertyName}</strong>.</p><label className="rental-request-comment-field">Comments<textarea autoFocus required maxLength={2000} rows={4} value={comments} onChange={(event) => setComments(event.target.value)} placeholder="Add comments for the property owner" /><small>{comments.trim().length}/2000 characters</small></label></>}</> : ''}
        onClose={() => { if (!submitting) setSelectedAction(null); }}
        actions={[
          { label: 'Go back', onClick: () => setSelectedAction(null), variant: 'secondary', disabled: submitting },
          { label: submitting ? 'Saving...' : isAccepting ? 'Accept request' : 'Send back to owner', onClick: respond, variant: 'primary', disabled: submitting || (!isAccepting && !canSendBack) },
        ]}
      />
    </main>
  );
}

function formatCurrency(amount: number): string {
  return new Intl.NumberFormat('en-IN', { style: 'currency', currency: 'INR', maximumFractionDigits: 0 }).format(amount);
}

function formatDate(value: string | null): string {
  return value ? new Date(value).toLocaleDateString('en-IN', { day: 'numeric', month: 'short', year: 'numeric' }) : 'Not specified';
}