import type { ReactElement } from 'react';
import { Building2, CreditCard, History, Mail, MessageSquare, TrendingUp, UserCheck } from 'lucide-react';

type ProfileRole = 'tenant' | 'owner' | 'both';
interface Action {
  title: string;
  description: string;
  linkText: string;
  path: string;
  tone: 'green' | 'purple' | 'blue' | 'gold';
  icon: ReactElement;
}

const ownerActions: Action[] = [
  { title: 'Map Tenant', description: 'Link tenants to your properties', linkText: 'Manage Mappings', path: '/owner/map-tenant', tone: 'green', icon: <UserCheck size={19} aria-hidden="true" /> },
  { title: 'Tenant Score', description: 'View tenant credit and profile scores', linkText: 'Check Scores', path: '/owner/tenant-scores', tone: 'purple', icon: <TrendingUp size={19} aria-hidden="true" /> },
  { title: 'Add Property', description: 'Register your property for tenant mapping', linkText: 'Manage Properties', path: '/owner/add-property', tone: 'blue', icon: <Building2 size={19} aria-hidden="true" /> },
];

const tenantActions: Action[] = [
  { title: 'Rental Requests', description: 'View and respond to rental offers', linkText: 'View Requests', path: '/tenant/rental-requests', tone: 'green', icon: <Mail size={19} aria-hidden="true" /> },
  { title: 'Payment Track Record', description: 'View and manage your monthly rent payments', linkText: 'View Payments', path: '/tenant/payments', tone: 'blue', icon: <CreditCard size={19} aria-hidden="true" /> },
  { title: 'Rental History', description: 'View past & current properties', linkText: 'View History', path: '/tenant/rental-history', tone: 'purple', icon: <History size={19} aria-hidden="true" /> },
];

const feedbackAction: Action = { title: 'Move-out Feedback', description: 'View feedback history or rate former owners and tenants', linkText: 'View Feedback', path: '/feedback', tone: 'gold', icon: <MessageSquare size={19} aria-hidden="true" /> };

export function ExplorerActionsGrid({ role }: { role: ProfileRole }): ReactElement {
  const actions = role === 'owner'
    ? [...ownerActions, feedbackAction]
    : role === 'tenant'
      ? [...tenantActions, feedbackAction]
      : [...ownerActions, ...tenantActions, feedbackAction];
  const title = role === 'both' ? 'Owner and Tenant Actions' : role === 'owner' ? 'Owner Actions' : 'Tenant Actions';

  return (
    <main className="mobile-content owner-actions-content">
      <section className="owner-actions-section" aria-labelledby="explorer-actions-title">
        <h2 id="explorer-actions-title">{title}</h2>
        <div className="owner-actions-grid">
          {actions.map((action) => <ActionTile key={action.path} action={action} />)}
        </div>
      </section>
    </main>
  );
}

function ActionTile({ action }: { action: Action }): ReactElement {
  return (
    <a className={`owner-action-tile ${action.tone}`} href={`#${action.path}`}>
      <span className="owner-action-icon">{action.icon}</span>
      <span className="owner-action-title">{action.title}</span>
      <span className="owner-action-description">{action.description}</span>
      <span className="owner-action-link">{action.linkText} <span aria-hidden="true">→</span></span>
    </a>
  );
}