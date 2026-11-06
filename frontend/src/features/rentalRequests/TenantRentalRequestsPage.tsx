import { useEffect, useState, type ReactElement } from 'react';
import { ArrowLeft, CalendarDays, Check, FileText, MapPin } from 'lucide-react';
import { ActionModal } from '../../components/ActionModal';
import { downloadRentalAgreement, getTenantRentalRequests, respondToRentalRequest } from '../../services/rentalRequestApi';
import type { TenantRentalRequestRecord } from '../../types/rentalRequest';

type RequestAction = 'accept' | 'reject';

export function TenantRentalRequestsPage({ onBack }: { onBack: () => void }): ReactElement {
  const [requests, setRequests] = useState<TenantRentalRequestRecord[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [selectedAction, setSelectedAction] = useState<{ request: TenantRentalRequestRecord; action: RequestAction } | null>(null);
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
      await respondToRentalRequest(selectedAction.request.leaseId, selectedAction.action);
      await loadRequests();
      setSelectedAction(null);
    } catch (reason) {
      setError(reason instanceof Error ? reason.message : 'Your response could not be saved.');
    } finally {
      setSubmitting(false);
    }
  }

  const isAccepting = selectedAction?.action === 'accept';

  return (
    <main className="mobile-content tenant-requests-page">
      <button className="properties-back" type="button" onClick={onBack}><ArrowLeft size={16} /> Services</button>
      <header className="properties-header">
        <div>
          <p className="mobile-kicker">Tenant workspace</p>
          <h1>Rental Requests ({requests.length})</h1>
        </div>
      </header>
      {error && <p className="properties-error" role="alert">{error}</p>}
      {loading ? <p className="mapping-loading">Loading rental requests...</p> : requests.length === 0 ? (
        <section className="mapping-empty-requests">
          <FileText size={30} />
          <h2>No Rental Requests</h2>
          <p>Requests from property owners will appear here.</p>
        </section>
      ) : (
        <section className="mapping-list" aria-label="Rental requests from property owners">
          {requests.map((request) => (
            <article className="tenant-rental-request-card" key={request.leaseId}>
              <header className="rental-request-heading">
                <div>
                  <h2>{request.propertyName}</h2>
                  <p><MapPin size={14} />{[request.address, request.city, request.state].filter(Boolean).join(', ')}</p>
                </div>
                <span className={`rental-request-status ${request.status.toLowerCase()}`}>{request.status}</span>
              </header>
              <dl className="tenant-request-details">
                <div><dt>Owner</dt><dd>{request.ownerName || 'Property owner'}</dd></div>
                <div><dt>Property type</dt><dd>{request.propertyType}</dd></div>
                <div><dt>Monthly rent</dt><dd>{formatCurrency(request.agreedRent)}</dd></div>
                <div><dt>Advance</dt><dd>{formatCurrency(request.agreedAdvance)}</dd></div>
                <div><dt>Requested dates</dt><dd><CalendarDays size={14} />{formatDate(request.startDate)} - {formatDate(request.endDate)}</dd></div>
                <div><dt>Request sent</dt><dd>{formatDate(request.createdAt)}</dd></div>
              </dl>
              <footer className="tenant-request-actions">
                {request.hasAgreementDocument && <button className="tenant-request-document" type="button" onClick={() => { void downloadRentalAgreement(request.leaseId).catch((reason: unknown) => setError(reason instanceof Error ? reason.message : 'Agreement document could not be opened.')); }}><FileText size={15} /> View agreement</button>}
                {request.status === 'Pending' && <div className="tenant-request-response-actions">
                  <button className="mapping-secondary-button" type="button" onClick={() => setSelectedAction({ request, action: 'reject' })}>Reject</button>
                  <button className="mapping-primary-button" type="button" onClick={() => setSelectedAction({ request, action: 'accept' })}><Check size={15} /> Accept</button>
                </div>}
              </footer>
            </article>
          ))}
        </section>
      )}
      <ActionModal
        isOpen={selectedAction !== null}
        title={isAccepting ? 'Accept rental request?' : 'Reject rental request?'}
        message={selectedAction ? <>{isAccepting ? 'Accept' : 'Reject'} the request for <strong>{selectedAction.request.propertyName}</strong> from {selectedAction.request.ownerName || 'the owner'}?</> : ''}
        onClose={() => { if (!submitting) setSelectedAction(null); }}
        actions={[
          { label: 'Go back', onClick: () => setSelectedAction(null), variant: 'secondary', disabled: submitting },
          { label: submitting ? 'Saving...' : isAccepting ? 'Accept request' : 'Reject request', onClick: respond, variant: isAccepting ? 'primary' : 'danger', disabled: submitting },
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