import { useEffect, useState, type ReactElement } from 'react';
import { ArrowLeft, CalendarDays, FileText, MapPin } from 'lucide-react';
import { getTenantRentalHistory, downloadRentalAgreement } from '../../services/rentalRequestApi';
import type { TenantRentalRequestRecord } from '../../types/rentalRequest';

export function TenantRentalHistoryPage({ onBack }: { onBack: () => void }): ReactElement {
  const [rentals, setRentals] = useState<TenantRentalRequestRecord[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [documentError, setDocumentError] = useState('');

  useEffect(() => {
    let mounted = true;
    getTenantRentalHistory()
      .then((result) => { if (mounted) setRentals(result); })
      .catch((reason: unknown) => { if (mounted) setError(reason instanceof Error ? reason.message : 'Rental history could not be loaded.'); })
      .finally(() => { if (mounted) setLoading(false); });
    return () => { mounted = false; };
  }, []);

  const currentRentals = rentals.filter((rental) => rental.status === 'Active' || rental.status === 'CancellationRequested');
  const previousRentals = rentals.filter((rental) => rental.status === 'Terminated');

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
          />
          <HistorySection
            title="Previous Rentals"
            category="Previous rental"
            rentals={previousRentals}
            emptyMessage="You have no previous rentals yet."
            onDownload={(leaseId) => { setDocumentError(''); void downloadRentalAgreement(leaseId).catch((reason: unknown) => setDocumentError(reason instanceof Error ? reason.message : 'Agreement document could not be opened.')); }}
          />
        </>
      )}
    </main>
  );
}

function HistorySection({
  title,
  category,
  rentals,
  emptyMessage,
  onDownload,
}: {
  title: string;
  category: string;
  rentals: TenantRentalRequestRecord[];
  emptyMessage: string;
  onDownload: (leaseId: string) => void;
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
                <span className={`rental-request-status ${rental.status.toLowerCase()}`}>{rental.status === 'CancellationRequested' ? 'Cancellation requested' : rental.status}</span>
              </header>
              <dl className="tenant-request-details">
                <div><dt>Property type</dt><dd>{rental.propertyType}</dd></div>
                <div><dt>Owner</dt><dd>{rental.ownerName || 'Property owner'}</dd></div>
                <div><dt>Monthly rent</dt><dd>{formatCurrency(rental.agreedRent)}</dd></div>
                <div><dt>Advance</dt><dd>{formatCurrency(rental.agreedAdvance)}</dd></div>
                <div><dt>Rental period</dt><dd><CalendarDays size={14} />{formatDate(rental.startDate)} - {formatDate(rental.endDate)}</dd></div>
                <div><dt>{category === 'Current rental' ? 'Started' : 'Rental ended'}</dt><dd>{formatDate(category === 'Current rental' ? rental.startDate : rental.endDate)}</dd></div>
              </dl>
              {rental.hasAgreementDocument && <footer className="tenant-history-card-footer"><button className="tenant-request-document" type="button" onClick={() => onDownload(rental.leaseId)}><FileText size={15} />View agreement</button></footer>}
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
