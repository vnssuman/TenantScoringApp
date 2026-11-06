import { useEffect, useState, type FormEvent, type ReactElement } from 'react';
import { ArrowLeft, FileText, MapPin, Paperclip, Plane, UserRound, Wallet } from 'lucide-react';
import { getProperties } from '../../services/propertyApi';
import { downloadRentalAgreement, getRentalRequests, searchTenants, sendRentalRequest, withdrawRentalRequest } from '../../services/rentalRequestApi';
import { getCurrentProfile } from '../../services/profileApi';
import { ActionModal } from '../../components/ActionModal';
import type { PropertyRecord } from '../../types/property';
import type { RentalRequestRecord, TenantLookupResult } from '../../types/rentalRequest';

type RequestTab = 'active' | 'sent';
type RequestStep = 1 | 2 | 3;

export function MapTenantPage({ onBack }: { onBack: () => void }): ReactElement {
  const [activeTab, setActiveTab] = useState<RequestTab>('active');
  const [activeRenters, setActiveRenters] = useState<RentalRequestRecord[]>([]);
  const [sentRequests, setSentRequests] = useState<RentalRequestRecord[]>([]);
  const [properties, setProperties] = useState<PropertyRecord[]>([]);
  const [isRequestOpen, setIsRequestOpen] = useState(false);
  const [step, setStep] = useState<RequestStep>(1);
  const [selectedProperty, setSelectedProperty] = useState<PropertyRecord | null>(null);
  const [tenantQuery, setTenantQuery] = useState('');
  const [tenantResults, setTenantResults] = useState<TenantLookupResult[]>([]);
  const [selectedTenant, setSelectedTenant] = useState<TenantLookupResult | null>(null);
  const [rentAmount, setRentAmount] = useState('');
  const [startDate, setStartDate] = useState('');
  const [endDate, setEndDate] = useState('');
  const [agreementDocument, setAgreementDocument] = useState<File | undefined>();
  const [loading, setLoading] = useState(true);
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState('');

  async function refreshRequests(): Promise<void> {
    const [active, pending] = await Promise.all([getRentalRequests('Active'), getRentalRequests('Pending')]);
    setActiveRenters(active);
    setSentRequests(pending);
  }

  useEffect(() => {
    let active = true;
    async function loadModule(): Promise<void> {
      try {
        const profile = await getCurrentProfile();
        const [ownerProperties, activeRequests, pendingRequests] = await Promise.all([
          getProperties(profile.userId),
          getRentalRequests('Active'),
          getRentalRequests('Pending'),
        ]);
        if (active) {
          setProperties(ownerProperties);
          setActiveRenters(activeRequests);
          setSentRequests(pendingRequests);
        }
      } catch (reason) {
        if (active) setError(reason instanceof Error ? reason.message : 'Map Tenant could not be loaded.');
      } finally {
        if (active) setLoading(false);
      }
    }
    void loadModule();
    return () => { active = false; };
  }, []);

  useEffect(() => {
    if (tenantQuery.trim().length < 2 || !isRequestOpen || step !== 2 || selectedTenant) {
      setTenantResults([]);
      return;
    }
    let active = true;
    const timer = window.setTimeout(() => {
      searchTenants(tenantQuery.trim()).then((matches) => { if (active) setTenantResults(matches); })
        .catch((reason: unknown) => { if (active) setError(reason instanceof Error ? reason.message : 'Tenant search failed.'); });
    }, 250);
    return () => { active = false; window.clearTimeout(timer); };
  }, [tenantQuery, isRequestOpen, step, selectedTenant]);

  function openRequestFlow(): void {
    setError('');
    setStep(1);
    setSelectedProperty(null);
    setTenantQuery('');
    setSelectedTenant(null);
    setRentAmount('');
    setStartDate('');
    setEndDate('');
    setAgreementDocument(undefined);
    setIsRequestOpen(true);
  }

  function closeRequestFlow(): void {
    setIsRequestOpen(false);
    setError('');
  }

  async function submitRequest(event: FormEvent<HTMLFormElement>): Promise<void> {
    event.preventDefault();
    if (!selectedProperty || !selectedTenant) return;
    setSubmitting(true);
    setError('');
    try {
      await sendRentalRequest({
        propertyId: selectedProperty.id,
        tenantId: selectedTenant.tenantId,
        rentAmount: Number(rentAmount),
        startDate,
        endDate,
        agreementDocument,
      });
      await refreshRequests();
      setActiveTab('sent');
      closeRequestFlow();
    } catch (reason) {
      setError(reason instanceof Error ? reason.message : 'Rental request could not be sent.');
    } finally {
      setSubmitting(false);
    }
  }

  async function withdrawRequest(leaseId: string): Promise<void> {
    await withdrawRentalRequest(leaseId);
    await refreshRequests();
  }

  const vacantProperties = properties.filter((property) => property.is_vacant);

  return (
    <main className="mobile-content map-tenant-page">
      <button className="properties-back" type="button" onClick={onBack}><MapPin size={16} /> Explorer</button>
      <header className="mapping-header">
        <div>
          <p className="mobile-kicker">Owner workspace</p>
          <h1>Map Tenant</h1>
        </div>
        {!isRequestOpen && <button className="mapping-send-button" type="button" onClick={openRequestFlow}><Plane size={16} /> Send New Request</button>}
      </header>

      {isRequestOpen ? (
        <section className="mapping-request-panel" aria-labelledby="mapping-flow-title">
            <header className="mapping-flow-header"><button className="mapping-flow-back" type="button" onClick={closeRequestFlow}><ArrowLeft size={16} /> Back to requests</button><p className="mobile-kicker">Step {step} of 3</p><h2 id="mapping-flow-title">Send Rental Request</h2></header>
            <div className="mapping-step-indicator" aria-hidden="true"><span className={step >= 1 ? 'current' : ''} /><span className={step >= 2 ? 'current' : ''} /><span className={step >= 3 ? 'current' : ''} /></div>
            {step === 1 && <section className="mapping-step-content"><h3>Select a vacant property</h3>{vacantProperties.length === 0 ? <div className="mapping-empty-inline">No vacant properties are available. Add a property or update its vacancy status first.</div> : <div className="mapping-property-options">{vacantProperties.map((property) => <button className={`mapping-property-option ${selectedProperty?.id === property.id ? 'selected' : ''}`} key={property.id} type="button" onClick={() => { setSelectedProperty(property); setRentAmount(String(property.monthly_rent)); }}><strong>{property.property_name}</strong><span>{property.address}, {property.city}, {property.state}</span><small>{formatCurrency(property.monthly_rent)} / month</small></button>)}</div>}<div className="mapping-flow-actions"><button className="mapping-primary-button" type="button" disabled={!selectedProperty} onClick={() => { setError(''); setStep(2); }}>Continue</button></div></section>}
            {step === 2 && <section className="mapping-step-content"><h3>Find a tenant</h3>{selectedTenant ? <div className="mapping-selected-tenant"><UserRound size={18} /><span><strong>{selectedTenant.fullName}</strong><small>Profile ID {formatProfileId(selectedTenant.profileId)}</small></span><button type="button" aria-label="Remove selected tenant" onClick={() => { setSelectedTenant(null); setTenantQuery(''); }}>Change</button></div> : <div className="tenant-search-wrap"><input autoFocus value={tenantQuery} placeholder="Search by tenant name or profile ID" onChange={(event) => { setTenantQuery(event.target.value); setError(''); }} /><div className="tenant-search-results">{tenantQuery.trim().length >= 2 && tenantResults.length === 0 && <p>No matching tenants found.</p>}{tenantResults.map((tenant) => <button key={tenant.tenantId} type="button" onClick={() => { setSelectedTenant(tenant); setTenantQuery(tenant.fullName); setError(''); }}><span>{tenant.fullName}</span><small>{formatProfileId(tenant.profileId)}</small></button>)}</div></div>}<div className="mapping-flow-actions"><button className="mapping-secondary-button" type="button" onClick={() => setStep(1)}>Back</button><button className="mapping-primary-button" type="button" disabled={!selectedTenant} onClick={() => setStep(3)}>Continue</button></div></section>}
            {step === 3 && <form className="mapping-step-content mapping-rental-form" onSubmit={submitRequest}><h3>Rental details</h3><div className="mapping-field-grid"><label>Monthly rent<input required type="number" min="1" step="1" value={rentAmount} onChange={(event) => setRentAmount(event.target.value)} placeholder={String(selectedProperty?.monthly_rent ?? '')} /></label><label>Start date<input required type="date" value={startDate} min={new Date().toISOString().slice(0, 10)} onChange={(event) => setStartDate(event.target.value)} /></label><label>End date<input required type="date" value={endDate} min={startDate || new Date().toISOString().slice(0, 10)} onChange={(event) => setEndDate(event.target.value)} /></label><label className="mapping-document-field">Agreement document (PDF, max 5 MB)<span className="mapping-file-control"><Paperclip size={15} /><input type="file" accept="application/pdf,.pdf" onChange={(event) => setAgreementDocument(event.target.files?.[0])} /></span>{agreementDocument && <small>{agreementDocument.name}</small>}</label></div>{error && <p className="properties-error" role="alert">{error}</p>}<div className="mapping-flow-actions"><button className="mapping-secondary-button" type="button" onClick={() => setStep(2)}>Back</button><button className="mapping-primary-button" type="submit" disabled={submitting}>{submitting ? 'Sending...' : 'Send Request'}</button></div></form>}
        </section>
      ) : (
        <>
          <div className="mapping-tabs" role="tablist" aria-label="Tenant mappings">
            <button className={activeTab === 'active' ? 'active' : ''} type="button" role="tab" aria-selected={activeTab === 'active'} onClick={() => setActiveTab('active')}>Active Renters ({activeRenters.length})</button>
            <button className={activeTab === 'sent' ? 'active' : ''} type="button" role="tab" aria-selected={activeTab === 'sent'} onClick={() => setActiveTab('sent')}>Sent Requests ({sentRequests.length})</button>
          </div>

          {error && <p className="properties-error" role="alert">{error}</p>}
          {loading ? <p className="mapping-loading">Loading tenant mappings...</p> : activeTab === 'active' ? (
            activeRenters.length === 0 ? <EmptyRenters onSend={openRequestFlow} /> : (
              <section className="mapping-list" aria-label="Active renters">
                {activeRenters.map((request) => <RequestCard key={request.leaseId} request={request} onDownload={() => downloadRentalAgreement(request.leaseId)} />)}
              </section>
            )
          ) : sentRequests.length === 0 ? (
            <section className="mapping-empty-requests"><FileText size={30} /><h2>No Sent Requests</h2><p>Rental requests you send will appear here.</p><button className="mapping-send-button" type="button" onClick={openRequestFlow}><Plane size={15} /> Send Request</button></section>
          ) : (
            <section className="mapping-list" aria-label="Sent rental requests">
                {sentRequests.map((request) => <RequestCard key={request.leaseId} request={request} onDownload={() => downloadRentalAgreement(request.leaseId)} onWithdraw={() => withdrawRequest(request.leaseId)} />)}
            </section>
          )}
        </>
      )}
    </main>
  );
}

function EmptyRenters({ onSend }: { onSend: () => void }): ReactElement {
  return <section className="mapping-empty-state"><span className="mapping-empty-icon"><UserRound size={24} /></span><h2>No Active Renters</h2><p>Send rental requests to renters to get started</p><button className="mapping-send-button" type="button" onClick={onSend}><Plane size={15} /> Send Request</button></section>;
}

function RequestCard({ request, onDownload, onWithdraw }: { request: RentalRequestRecord; onDownload: () => Promise<void>; onWithdraw?: () => Promise<void> }): ReactElement {
  const [downloadError, setDownloadError] = useState('');
  const [withdrawError, setWithdrawError] = useState('');
  const [withdrawing, setWithdrawing] = useState(false);
  const [isWithdrawModalOpen, setIsWithdrawModalOpen] = useState(false);
  async function handleWithdraw(): Promise<void> {
    if (!onWithdraw) return;
    setWithdrawing(true);
    setWithdrawError('');
    try {
      await onWithdraw();
      setIsWithdrawModalOpen(false);
    } catch (reason) {
      setWithdrawError(reason instanceof Error ? reason.message : 'Request could not be withdrawn.');
    } finally {
      setWithdrawing(false);
    }
  }
  return <article className="rental-request-card"><header className="rental-request-heading"><div><h2>{request.propertyName}</h2><p><MapPin size={14} />{request.location}</p></div><span className={`rental-request-status ${request.status.toLowerCase()}`}>{request.status}</span></header><div className="rental-request-metadata"><div><span>Profile ID</span><strong className="mapping-profile-id">{formatProfileId(request.profileId)}</strong></div><div><span>Monthly Rent</span><strong>{formatCurrency(request.agreedRent)}</strong></div><div><span>Start Date</span><strong>{formatDate(request.startDate)}</strong></div><div><span>End Date</span><strong>{formatDate(request.endDate)}</strong></div></div>{(request.hasAgreementDocument || onWithdraw) && <footer className="rental-request-footer">{request.hasAgreementDocument && <button type="button" onClick={() => { setDownloadError(''); void onDownload().catch((reason: unknown) => setDownloadError(reason instanceof Error ? reason.message : 'Document download failed.')); }}><FileText size={16} />Mock Agreement Document - Standard Rental Agreement.pdf</button>}{downloadError && <small role="alert">{downloadError}</small>}{onWithdraw && <button className="mapping-withdraw-button" type="button" disabled={withdrawing} onClick={() => { setWithdrawError(''); setIsWithdrawModalOpen(true); }}>Withdraw request</button>}{withdrawError && <small role="alert">{withdrawError}</small>}</footer>}
    <ActionModal
      isOpen={isWithdrawModalOpen}
      title="Withdraw rental request?"
      message={<>{'This request will be withdrawn and removed from your sent requests.'}{withdrawError && <p className="action-modal-error" role="alert">{withdrawError}</p>}</>}
      onClose={() => { if (!withdrawing) setIsWithdrawModalOpen(false); }}
      actions={[
        { label: 'Keep request', onClick: () => setIsWithdrawModalOpen(false), variant: 'secondary', disabled: withdrawing },
        { label: withdrawing ? 'Withdrawing...' : 'Withdraw request', onClick: handleWithdraw, variant: 'danger', disabled: withdrawing },
      ]}
    />
  </article>;
}

function formatProfileId(profileId: string): string { return `TNT${profileId.replaceAll('-', '').slice(0, 8).toUpperCase()}`; }
function formatCurrency(amount: number): string { return new Intl.NumberFormat('en-IN', { style: 'currency', currency: 'INR', maximumFractionDigits: 0 }).format(amount); }
function formatDate(value: string | null): string { return value ? new Date(value).toLocaleDateString('en-GB') : 'Not set'; }