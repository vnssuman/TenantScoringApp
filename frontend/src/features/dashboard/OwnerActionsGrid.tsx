import type { ReactElement } from 'react';
import { Building2, MessageSquare, TrendingUp, UserCheck } from 'lucide-react';

interface OwnerAction {
  title: string;
  description: string;
  linkText: string;
  path: string;
  tone: 'green' | 'purple' | 'blue' | 'gold';
  icon: ReactElement;
}

const ownerActions: OwnerAction[] = [
  {
    title: 'Map Tenant',
    description: 'Link tenants to your properties',
    linkText: 'Manage Mappings',
    path: '/owner/map-tenant',
    tone: 'green',
    icon: <UserCheck size={19} aria-hidden="true" />,
  },
  {
    title: 'Tenant Score',
    description: 'View tenant credit and profile scores',
    linkText: 'Check Scores',
    path: '/owner/tenant-scores',
    tone: 'purple',
    icon: <TrendingUp size={19} aria-hidden="true" />,
  },
  {
    title: 'Add Property',
    description: 'Register your property for tenant mapping',
    linkText: 'Manage Properties',
    path: '/owner/add-property',
    tone: 'blue',
    icon: <Building2 size={19} aria-hidden="true" />,
  },
  {
    title: 'Tenant Feedback',
    description: 'Rate and review your current tenants',
    linkText: 'Give Feedback',
    path: '/owner/feedback',
    tone: 'gold',
    icon: <MessageSquare size={19} aria-hidden="true" />,
  },
];

export function OwnerActionsGrid(): ReactElement {
  return (
    <main className="mobile-content owner-actions-content">
      <section className="owner-actions-section" aria-labelledby="owner-actions-title">
        <h2 id="owner-actions-title">Owner Actions</h2>
        <div className="owner-actions-grid">
          {ownerActions.map((action) => <ActionTile key={action.path} action={action} />)}
        </div>
      </section>
    </main>
  );
}

function ActionTile({ action }: { action: OwnerAction }): ReactElement {
  return (
    <a className={`owner-action-tile ${action.tone}`} href={`#${action.path}`}>
      <span className="owner-action-icon">{action.icon}</span>
      <span className="owner-action-title">{action.title}</span>
      <span className="owner-action-description">{action.description}</span>
      <span className="owner-action-link">{action.linkText} <span aria-hidden="true">→</span></span>
    </a>
  );
}