import { useEffect, useState, type ReactElement } from 'react';
import { ArrowLeft, CalendarDays, Check, FileText, MapPin } from 'lucide-react';
import { ActionModal } from '../../components/ActionModal';
import { getTenantRentalHistory, downloadRentalAgreement, respondToRentalRequest } from '../../services/rentalRequestApi';
import type { TenantRentalRequestRecord } from '../../types/rentalRequest';

export function TenantRentalHistoryPage({ onBack, onPendingCancellationCountChanged }: { onBack: () => void; onPendingCancellationCountChanged: (count: number) => void }): ReactElement {
  const [rentals, setRentals] = useState<TenantRentalRequestRecord[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [documentError, setDocumentError] = useState('');
  const [cancellationLeaseId, setCancellationLeaseId] = useState<string | null>(null);
  const [cancellationSubmitting, setCancellationSubmitting] = useState(false);

  useEffect(() => {
    let mounted = true;
    getTenantRentalHistory()
      .then((result) => {
        if (mounted) {
          setRentals(result);
          onPendingCancellationCountChanged(result.filter((rental) => rental.status === 'OwnerCancellationRequested').length);
        }
      })
      .catch((reason: unknown) => { if (mounted) setError(reason instanceof Error ? reason.message : 'Rental history could not be loaded.'); })
      .finally(() => { if (mounted) setLoading(false); });
    return () => { mounted = false; };
  }, []);

  const currentRentals = rentals.filter((rental) => rental.status === 'Active' || rental.status === 'CancellationRequested' || rental.status === 'OwnerCancellationRequested');
  const previousRentals = rentals.filter((rental) => rental.status === 'Terminated');
  const pendingCancellation = rentals.find((rental) => rental.leaseId === cancellationLeaseId);

  async function acceptCancellation(): Promise<void> {
    if (!cancellationLeaseId) return;
    setCancellationSubmitting(true);
    setError('');
    try {
      await respondToRentalRequest(cancellationLeaseId, 'accept-cancellation');
      const updatedRentals = rentals.map((rental) => rental.leaseId === cancellationLeaseId ? { ...rental, status: 'Terminated' } : rental);
      setRentals(updatedRentals);
      onPendingCancellationCountChanged(updatedRentals.filter((rental) => rental.status === 'OwnerCancellationRequested').length);
      setCancellationLeaseId(null);
      const refreshedRentals = await getTenantRentalHistory();
      setRentals(refreshedRentals);
      onPendingCancellationCountChanged(refreshedRentals.filter((rental) => rental.status === 'OwnerCancellationRequested').length);
    } catch (reason) {
      setError(reason instanceof Error ? reason.message : 'Tenancy cancellation could not be accepted.');
    } finally {
      setCancellationSubmitting(false);
    }
  }

  return (
    <main className="mobile-content tenant-history-page">
      <button className="properties-back" type="button" onClick={onBack}><ArrowLeft size={16} /> Services</button>
      <header className="properties-header">
        <div>
          <p className="mobile-kicker">Tenant workspace</p>
          <h1>Rental History</h1>
        </div>
      </header>
      {error && <p className="properties-error" role="alert">{error}</p>}
      {documentError && <p className="properties-error" role="alert">{documentError}</p>}
      {loading ? <p className="mapping-loading">Loading rental history...</p> : (
        <>
          <HistorySection
            title="Current Rentals"
            category="Current rental"
            rentals={currentRentals}
            emptyMessage="You have no current rentals."
            onDownload={(leaseId) => { setDocumentError(''); void downloadRentalAgreement(leaseId).catch((reason: unknown) => setDocumentError(reason instanceof Error ? reason.message : 'Agreement document could not be opened.')); }}
            onAcceptCancellation={(leaseId) => setCancellationLeaseId(leaseId)}
          />
          <HistorySection
            title="Previous Rentals"
            category="Previous rental"
            rentals={previousRentals}
            emptyMessage="You have no previous rentals yet."
            onDownload={(leaseId) => { setDocumentError(''); void downloadRentalAgreement(leaseId).catch((reason: unknown) => setDocumentError(reason instanceof Error ? reason.message : 'Agreement document could not be opened.')); }}
            onAcceptCancellation={() => undefined}
          />
        </>
      )}
      <ActionModal
        isOpen={pendingCancellation !== undefined}
        title="Accept tenancy cancellation?"
        message={pendingCancellation ? <>Accept the owner&apos;s request to end your tenancy at <strong>{pendingCancellation.propertyName}</strong>? This will end the tenancy and enable feedback for both of you.</> : ''}
        onClose={() => { if (!cancellationSubmitting) setCancellationLeaseId(null); }}
        actions={[
          { label: 'Keep tenancy', onClick: () => setCancellationLeaseId(null), variant: 'secondary', disabled: cancellationSubmitting },
          { label: cancellationSubmitting ? 'Cancelling...' : 'Accept cancellation', onClick: acceptCancellation, variant: 'primary', disabled: cancellationSubmitting },
        ]}
      />
    </main>
  );
}

function HistorySection({
  title,
  category,
  rentals,
  emptyMessage,
  onDownload,
  onAcceptCancellation,
}: {
  title: string;
  category: string;
  rentals: TenantRentalRequestRecord[];
  emptyMessage: string;
  onDownload: (leaseId: string) => void;
  onAcceptCancellation: (leaseId: string) => void;
}): ReactElement {
  return (
    <section className="tenant-history-section" aria-label={title}>
      <header className="tenant-history-section-heading">
        <h2>{title}</h2>
        <span>{rentals.length}</span>
      </header>
      {rentals.length === 0 ? <p className="tenant-history-empty">{emptyMessage}</p> : (
        <div className="mapping-list">
          {rentals.map((rental) => (
            <article className={`tenant-history-card ${category === 'Current rental' ? 'current' : 'previous'}`} key={rental.leaseId}>
              <header className="rental-request-heading">
                <div>
                  <span className="tenant-history-category">{category}</span>
                  <h3>{rental.propertyName}</h3>
                  <p><MapPin size={14} />{[rental.address, rental.city, rental.state].filter(Boolean).join(', ') || rental.propertyType}</p>
                </div>
                <span className={`rental-request-status ${rental.status.toLowerCase()}`}>{rental.status === 'CancellationRequested' ? 'Cancellation requested' : rental.status === 'OwnerCancellationRequested' ? 'Owner requested cancellation' : rental.status}</span>
              </header>
              <dl className="tenant-request-details">
                <div><dt>Property type</dt><dd>{rental.propertyType}</dd></div>
                <div><dt>Owner</dt><dd>{rental.ownerName || 'Property owner'}</dd></div>
                <div><dt>Monthly rent</dt><dd>{formatCurrency(rental.agreedRent)}</dd></div>
                <div><dt>Advance</dt><dd>{formatCurrency(rental.agreedAdvance)}</dd></div>
                <div><dt>Rental period</dt><dd><CalendarDays size={14} />{formatDate(rental.startDate)} - {formatDate(rental.endDate)}</dd></div>
                <div><dt>{category === 'Current rental' ? 'Started' : 'Rental ended'}</dt><dd>{formatDate(category === 'Current rental' ? rental.startDate : rental.endDate)}</dd></div>
              </dl>
              {(rental.hasAgreementDocument || rental.status === 'OwnerCancellationRequested') && <footer className="tenant-history-card-footer">{rental.hasAgreementDocument && <button className="tenant-request-document" type="button" onClick={() => onDownload(rental.leaseId)}><FileText size={15} />View agreement</button>}{rental.status === 'OwnerCancellationRequested' && <button className="mapping-primary-button" type="button" onClick={() => onAcceptCancellation(rental.leaseId)}><Check size={15} />Accept cancellation</button>}</footer>}
            </article>
          ))}
        </div>
      )}
    </section>
  );
}

function formatCurrency(amount: number): string {
  return new Intl.NumberFormat('en-IN', { style: 'currency', currency: 'INR', maximumFractionDigits: 0 }).format(amount);
}

function formatDate(value: string | null): string {
  return value ? new Date(value).toLocaleDateString('en-IN', { day: 'numeric', month: 'short', year: 'numeric' }) : 'Not specified';
}
