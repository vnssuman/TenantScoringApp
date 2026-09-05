import { useEffect, useState, type ReactElement } from 'react';
import {
  Building2,
  ChevronRight,
  LogOut,
  MessageCircle,
  Network,
  RefreshCw,
  ShieldCheck,
  Sparkles,
  TrendingUp,
  Users,
} from 'lucide-react';
import { getDashboardSummary } from '../../services/dashboardApi';
import type { DashboardAction, DashboardUserSummary, DashboardView } from '../../types/dashboard';

interface ScoreDashboardPageProps {
  onLogout: () => void;
}

const ownerActions: DashboardAction[] = [
  { title: 'Map Tenant', description: 'Link tenants to your properties', linkLabel: 'Manage Mappings', tone: 'green' },
  { title: 'Tenant Score', description: 'View tenant credit and profile scores', linkLabel: 'Check Scores', tone: 'purple' },
  { title: 'Add Property', description: 'Register your property for tenant mapping', linkLabel: 'Manage Properties', tone: 'blue' },
  { title: 'Tenant Feedback', description: 'Rate and review your current tenants', linkLabel: 'Give Feedback', tone: 'gold' },
];

const renterActions: DashboardAction[] = [
  { title: 'My Score', description: 'Review your current profile score', linkLabel: 'View Score', tone: 'purple' },
  { title: 'Find a Property', description: 'Explore properties available to you', linkLabel: 'Browse Properties', tone: 'blue' },
];

export function ScoreDashboardPage({ onLogout }: ScoreDashboardPageProps): ReactElement {
  const [summary, setSummary] = useState<DashboardUserSummary | null>(null);
  const [activeView, setActiveView] = useState<DashboardView>('owner');
  const [error, setError] = useState('');

  useEffect(() => {
    const token = localStorage.getItem('kirayeeToken');
    if (!token) {
      setError('Your session has expired. Please log in again.');
      return;
    }

    getDashboardSummary(token)
      .then(setSummary)
      .catch((reason: unknown) => setError(reason instanceof Error ? reason.message : 'Dashboard could not be loaded.'));
  }, []);

  if (error) {
    return (
      <section className="dashboard-state" role="alert">
        <ShieldCheck size={34} aria-hidden="true" />
        <h1>Session unavailable</h1>
        <p>{error}</p>
        <button className="dashboard-primary-button" type="button" onClick={onLogout}>Back to Login</button>
      </section>
    );
  }

  if (!summary) {
    return <section className="dashboard-state" aria-live="polite"><RefreshCw className="spin" size={30} aria-hidden="true" /><p>Loading your dashboard...</p></section>;
  }

  const firstName = summary.fullName.split(' ')[0];
  const actions = activeView === 'owner' ? ownerActions : renterActions;

  return (
    <section className="dashboard-page" aria-label="Score Dashboard">
      <header className="dashboard-header">
        <div>
          <div className="dashboard-brand"><span className="dashboard-brand-mark">K</span>Kirayee</div>
          <p className="dashboard-kicker">Score Dashboard</p>
          <h1>Welcome, {summary.fullName}</h1>
        </div>
        <button className="logout-button" type="button" onClick={onLogout}><LogOut size={16} aria-hidden="true" /> Logout</button>
      </header>

      <div className="profile-summary-grid">
        <ProfileCard label="Kirayee ID" value={summary.tenantId} accent="accent" />
        <ProfileCard label="Role" value={summary.assignedRoles.join(' / ')} />
        <ProfileCard label="Mobile Number" value={summary.mobileNumber} />
        <ProfileCard label="Email" value={summary.email} />
      </div>

      <section className="switch-banner" aria-label="Dashboard view switcher">
        <div className="switch-heading"><div className="switch-icon"><Sparkles size={19} aria-hidden="true" /></div><div><strong>Switch View</strong><span>Select which dashboard you want to view</span></div></div>
        <div className="view-toggle" role="group" aria-label="Dashboard view">
          <button className={activeView === 'owner' ? 'active' : ''} type="button" onClick={() => setActiveView('owner')}>Owner View</button>
          <button className={activeView === 'renter' ? 'active' : ''} type="button" onClick={() => setActiveView('renter')}>Renter View</button>
        </div>
      </section>

      <section className="actions-section" aria-labelledby="actions-title">
        <div className="section-heading"><div><p className="dashboard-kicker">Workspace</p><h2 id="actions-title">{activeView === 'owner' ? 'Owner Actions' : 'Renter Actions'}</h2></div><span className="action-count">{actions.length} available</span></div>
        <div className="actions-grid">
          {actions.map((action) => <ActionCard key={action.title} action={action} />)}
        </div>
      </section>
    </section>
  );
}

interface ProfileCardProps { label: string; value: string; accent?: string; }
function ProfileCard({ label, value, accent }: ProfileCardProps): ReactElement {
  return <article className="profile-card"><span>{label}</span><strong className={accent}>{value}</strong></article>;
}

interface ActionCardProps { action: DashboardAction; }
function ActionCard({ action }: ActionCardProps): ReactElement {
  const Icon = action.tone === 'green' ? Network : action.tone === 'purple' ? TrendingUp : action.tone === 'blue' ? Building2 : MessageCircle;
  return <article className="action-card"><div className={`action-icon ${action.tone}`}><Icon size={21} aria-hidden="true" /></div><h3>{action.title}</h3><p>{action.description}</p><button className={`action-link ${action.tone}`} type="button">{action.linkLabel}<ChevronRight size={15} aria-hidden="true" /></button></article>;
}
