import { useEffect, useState, type ReactElement } from 'react';
import { ArrowLeft, Award, CheckCircle2, CreditCard, MapPin } from 'lucide-react';
import { ActionModal } from '../../components/ActionModal';
import { getTenantPaymentRecords, payTenantRent } from '../../services/rentalRequestApi';
import type { TenantPaymentPropertyRecord, TenantRentPaymentRecord } from '../../types/rentalRequest';

export function TenantPaymentRecordPage({ onBack }: { onBack: () => void }): ReactElement {
  const [properties, setProperties] = useState<TenantPaymentPropertyRecord[]>([]);
  const [selectedLeaseId, setSelectedLeaseId] = useState('');
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [paymentToConfirm, setPaymentToConfirm] = useState<TenantRentPaymentRecord | null>(null);
  const [paying, setPaying] = useState(false);
  const [paymentError, setPaymentError] = useState('');

  async function refreshPayments(): Promise<void> {
    const records = await getTenantPaymentRecords();
    setProperties(records);
  }

  async function confirmPayment(): Promise<void> {
    if (!paymentToConfirm) return;
    setPaying(true);
    setPaymentError('');
    try {
      const demoReference = `DEMO-${crypto.randomUUID()}`;
      await payTenantRent(paymentToConfirm.paymentId, paymentToConfirm.amount, demoReference);
      await refreshPayments();
      setPaymentToConfirm(null);
    } catch (reason) {
      setPaymentError(reason instanceof Error ? reason.message : 'Payment could not be recorded.');
    } finally {
      setPaying(false);
    }
  }

  useEffect(() => {
    let mounted = true;
    getTenantPaymentRecords()
      .then((records) => {
        if (!mounted) return;
        setProperties(records);
        setSelectedLeaseId(records[0]?.leaseId ?? '');
      })
      .catch((reason: unknown) => { if (mounted) setError(reason instanceof Error ? reason.message : 'Payment records could not be loaded.'); })
      .finally(() => { if (mounted) setLoading(false); });
    return () => { mounted = false; };
  }, []);

  const selectedProperty = properties.find((property) => property.leaseId === selectedLeaseId) ?? null;
  const payments = selectedProperty?.payments ?? [];
  const paymentsMade = payments.filter((payment) => payment.status === 'Paid').length;
  const rewards = payments.reduce((total, payment) => total + payment.rewardPointsEarned, 0);
  const paymentRate = payments.length === 0 ? 0 : Math.round((paymentsMade / payments.length) * 100);

  return (
    <main className="mobile-content payment-record-page">
      <button className="properties-back" type="button" onClick={onBack}><ArrowLeft size={16} /> Services</button>
      <header className="properties-header">
        <div>
          <p className="mobile-kicker">Tenant workspace</p>
          <h1>Payment Track Record</h1>
        </div>
      </header>
      {error && <p className="properties-error" role="alert">{error}</p>}
      {loading ? <p className="mapping-loading">Loading payment records...</p> : properties.length === 0 ? (
        <section className="payment-empty-state">
          <CreditCard size={30} />
          <h2>No rental payment history</h2>
          <p>Payment records will appear after you accept a rental request.</p>
        </section>
      ) : (
        <>
          <section className="payment-property-select">
            <label htmlFor="payment-property">Select Property</label>
            <select id="payment-property" value={selectedLeaseId} onChange={(event) => setSelectedLeaseId(event.target.value)}>
              {properties.map((property) => <option key={property.leaseId} value={property.leaseId}>{property.propertyName} - {property.location} ({formatDate(property.startDate)})</option>)}
            </select>
            {selectedProperty && <p><MapPin size={14} />{selectedProperty.propertyType} · {selectedProperty.leaseStatus}</p>}
          </section>

          {selectedProperty && <>
            <section className="payment-summary-grid" aria-label="Payment summary">
              <SummaryCard icon={<CheckCircle2 size={21} />} label="Payments Made" value={String(paymentsMade)} tone="green" />
              <SummaryCard icon={<Award size={21} />} label="Total Rewards" value={String(rewards)} tone="gold" />
              <SummaryCard icon={<CreditCard size={21} />} label="Payment Rate" value={`${paymentRate}%`} tone="blue" />
            </section>
            {payments.length === 0 ? <p className="payment-no-rows">There are no scheduled payments for this rental period.</p> : (
              <div className="payment-table-scroll">
                <table className="payment-record-table">
                  <thead><tr><th>S.No</th><th>Month</th><th>Due Date</th><th>Paid Date</th><th>Amount</th><th>Payment Status</th><th>Rewards Earned</th></tr></thead>
                  <tbody>
                    {payments.map((payment, index) => <PaymentRow key={payment.paymentId} payment={payment} index={index} onPay={() => { setPaymentError(''); setPaymentToConfirm(payment); }} />)}
                  </tbody>
                </table>
              </div>
            )}
          </>}
        </>
      )}
      <ActionModal
        isOpen={paymentToConfirm !== null}
        title="Record demo payment?"
        message={<>This records {paymentToConfirm ? formatCurrency(paymentToConfirm.amount) : ''} as paid in the demo app. No real money will be transferred.{paymentError && <p className="action-modal-error" role="alert">{paymentError}</p>}</>}
        onClose={() => { if (!paying) setPaymentToConfirm(null); }}
        actions={[
          { label: 'Cancel', onClick: () => setPaymentToConfirm(null), variant: 'secondary', disabled: paying },
          { label: paying ? 'Processing...' : 'Confirm payment', onClick: confirmPayment, variant: 'primary', disabled: paying },
        ]}
      />
    </main>
  );
}

function SummaryCard({ icon, label, value, tone }: { icon: ReactElement; label: string; value: string; tone: 'green' | 'gold' | 'blue' }): ReactElement {
  return <article className={`payment-summary-card ${tone}`}><span className="payment-summary-icon">{icon}</span><div><span>{label}</span><strong>{value}</strong></div></article>;
}

function PaymentRow({ payment, index, onPay }: { payment: TenantRentPaymentRecord; index: number; onPay: () => void }): ReactElement {
  const paid = payment.status === 'Paid';
  return <tr>
    <td>{index + 1}</td>
    <td className="payment-month">{new Date(payment.dueDate).toLocaleDateString('en-IN', { month: 'short', year: 'numeric' })}</td>
    <td>{formatDate(payment.dueDate)}</td>
    <td>{formatDate(payment.paidDate)}</td>
    <td className="payment-amount">{formatCurrency(payment.amount)}</td>
    <td>{paid ? <span className="payment-status paid"><CheckCircle2 size={15} />Paid</span> : payment.status === 'Pending' ? <button className="payment-pay-button" type="button" onClick={onPay}>Pay now</button> : <span className="payment-status pending">{payment.status}</span>}</td>
    <td><span className="payment-rewards"><Award size={15} />+{payment.rewardPointsEarned}</span></td>
  </tr>;
}

function formatCurrency(amount: number): string {
  return new Intl.NumberFormat('en-IN', { style: 'currency', currency: 'INR', maximumFractionDigits: 0 }).format(amount);
}

function formatDate(value: string | null): string {
  return value ? new Date(value).toLocaleDateString('en-IN', { day: '2-digit', month: '2-digit', year: 'numeric' }) : '—';
}
