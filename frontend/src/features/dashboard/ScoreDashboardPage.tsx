import { useEffect, useState, type ReactElement } from 'react';
import { Bell, Building2, ChevronRight, CircleUserRound, CreditCard, Home, LogOut, RefreshCw, ShieldCheck, Sparkles, WalletCards } from 'lucide-react';
import { getDashboardSummary } from '../../services/dashboardApi';
import { getCurrentProfile } from '../../services/profileApi';
import type { DashboardUserSummary } from '../../types/dashboard';
import type { UserProfile } from '../../types/profile';
import { MapTenantPage } from '../mapping/MapTenantPage';
import { OwnerPropertiesPage } from '../properties/OwnerPropertiesPage';
import { ExplorerActionsGrid } from './ExplorerActionsGrid';

interface ScoreDashboardPageProps { onLogout: () => void; showProperties?: boolean; showMapTenant?: boolean; }
type Tab = 'home' | 'explore' | 'activity' | 'profile';
type Role = 'tenant' | 'owner';

export function ScoreDashboardPage({ onLogout, showProperties = false, showMapTenant = false }: ScoreDashboardPageProps): ReactElement {
  const [summary, setSummary] = useState<DashboardUserSummary | null>(null);
  const [error, setError] = useState('');
  const [tab, setTab] = useState<Tab>('home');
  const [role, setRole] = useState<Role>('tenant');
  const [profile, setProfile] = useState<UserProfile | null>(null);

  useEffect(() => {
    const token = localStorage.getItem('kirayeeToken'); 
    if (!token) { setError('Your session has expired. Please log in again.'); return; }
    getDashboardSummary(token).then(setSummary).catch((reason: unknown) => setError(reason instanceof Error ? reason.message : 'Dashboard could not be loaded.'));
    getCurrentProfile().then((currentProfile) => {
      setProfile(currentProfile);
      if (currentProfile.currentRole.toLowerCase() !== 'both') {
        setRole(currentProfile.currentRole.toLowerCase() === 'owner' ? 'owner' : 'tenant');
      }
    }).catch(() => undefined);
  }, []);

  function navigateToTab(nextTab: Tab): void {
    setTab(nextTab);
    if (showProperties || showMapTenant) window.location.hash = 'home';
  }

  if (error) return <section className="dashboard-state" role="alert"><ShieldCheck size={34} aria-hidden="true" /><h1>Session unavailable</h1><p>{error}</p><button className="dashboard-primary-button" type="button" onClick={onLogout}>Back to Login</button></section>;
  if (!summary) return <section className="dashboard-state" aria-live="polite"><RefreshCw className="spin" size={30} aria-hidden="true" /><p>Loading your Kirayee space...</p></section>;

  return <section className="mobile-dashboard" aria-label="Kirayee dashboard">
    <header className="mobile-header"><div className="dashboard-brand"><span className="dashboard-brand-mark">K</span><span>Kirayee</span></div><div className="header-actions">{profile && !profile.profileStatus.isProfileComplete && <button className="completion-chip" type="button" onClick={() => { window.location.hash = 'update-profile'; }}>Profile {profile.profileStatus.completionPercentage}%</button>}<button className="icon-button" type="button" aria-label="Notifications"><Bell size={19} /></button><button className="icon-button" type="button" aria-label="Log out" onClick={onLogout}><LogOut size={18} /></button></div></header>
    {profile && !profile.profileStatus.isProfileComplete && <button className="profile-alert" type="button" onClick={() => { window.location.hash = 'update-profile'; }}><ShieldCheck size={17} /><span><strong>Complete your profile</strong><small>{profile.profileStatus.completionPercentage}% complete · {profile.profileStatus.missingFields.join(', ')} pending</small></span><ChevronRight size={17} /></button>}
      {showMapTenant
        ? <MapTenantPage onBack={() => { setTab('explore'); window.location.hash = 'home'; }} />
        : showProperties
        ? <OwnerPropertiesPage onBack={() => { setTab('explore'); window.location.hash = 'home'; }} />
        : tab === 'home' && <HomeTab summary={summary} role={role} showRoleSwitch={profile?.currentRole.toLowerCase() === 'both'} onRoleChange={setRole} />}
      {!showProperties && !showMapTenant && tab === 'explore' && (profile
        ? <ExplorerActionsGrid role={profile.currentRole.toLowerCase() === 'both' ? 'both' : role} />
        : <section className="dashboard-state" aria-live="polite"><RefreshCw className="spin" size={24} aria-hidden="true" /><p>Loading your profile...</p></section>)}
    {!showProperties && !showMapTenant && tab === 'activity' && <SimpleTab icon={<WalletCards size={26} />} title="Your activity" copy="Payments, proposals, and lease updates will appear here." action="View payment history" />}
    {!showProperties && !showMapTenant && tab === 'profile' && <ProfileTab summary={summary} profile={profile} />}
    <nav className="bottom-nav" aria-label="Main navigation"><NavButton active={!showProperties && !showMapTenant && tab === 'home'} icon={<Home size={20} />} label="Home" onClick={() => navigateToTab('home')} /><NavButton active={showProperties || showMapTenant || tab === 'explore'} icon={<Building2 size={20} />} label="Explore" onClick={() => navigateToTab('explore')} /><NavButton active={!showProperties && !showMapTenant && tab === 'activity'} icon={<CreditCard size={20} />} label="Activity" onClick={() => navigateToTab('activity')} /><NavButton active={!showProperties && !showMapTenant && tab === 'profile'} icon={<CircleUserRound size={20} />} label="Profile" onClick={() => navigateToTab('profile')} /></nav>
  </section>;
}

function HomeTab({ summary, role, showRoleSwitch, onRoleChange }: { summary: DashboardUserSummary; role: Role; showRoleSwitch: boolean; onRoleChange: (role: Role) => void }): ReactElement {
  return <main className="mobile-content"><div className="welcome-row"><div><p className="mobile-kicker">Good morning</p><h1>{summary.fullName.split(' ')[0]}</h1></div><div className="avatar">{summary.fullName.charAt(0)}</div></div>{showRoleSwitch && <div className="role-switch" role="group" aria-label="Active role"><button className={role === 'tenant' ? 'active' : ''} type="button" onClick={() => onRoleChange('tenant')}>I’m a tenant</button><button className={role === 'owner' ? 'active' : ''} type="button" onClick={() => onRoleChange('owner')}>I’m an owner</button></div>}<div className="section-heading mobile-section-heading"><div><p className="mobile-kicker">Overview</p><h2>{role === 'tenant' ? 'Your rental readiness' : 'Your rental space'}</h2></div><ChevronRight size={18} /></div><div className="overview-grid"><OverviewCard icon={<ShieldCheck size={18} />} label="Overall Score" value={summary.overallScore === undefined ? 'Not available' : `${summary.overallScore}/100`} tone="mint" /><OverviewCard icon={<Sparkles size={18} />} label="Rewards" value="120 pts" tone="yellow" /></div><section className="next-step"><div className="next-step-icon"><Sparkles size={18} /></div><div><p className="mobile-kicker">Next step</p><strong>Complete your identity checks</strong><span>Verify PAN and Aadhaar to unlock all features.</span></div><ChevronRight size={18} /></section></main>;
}

function ProfileTab({ summary, profile }: { summary: DashboardUserSummary; profile: UserProfile | null }): ReactElement { return <main className="mobile-content"><p className="mobile-kicker">Account</p><h1 className="tab-title">Your profile</h1><section className="profile-intro"><div className="large-avatar">{summary.fullName.charAt(0)}</div><div><h2>{summary.fullName}</h2><p>{summary.email}</p></div></section><div className="profile-list"><ProfileRow label="Kirayee ID" value={summary.tenantId} /><ProfileRow label="Mobile number" value={summary.mobileNumber} /><ProfileRow label="Roles" value={summary.assignedRoles.join(' / ')} /></div><div className="settings-links"><button type="button" onClick={() => { window.location.hash = 'update-profile'; }}><ShieldCheck size={17} /><span>Update identity documents<small>{profile?.profileStatus.isPanVerified && profile.profileStatus.isAadhaarVerified ? 'Complete' : 'Pending verification'}</small></span><ChevronRight size={17} /></button><button type="button" onClick={() => { window.location.hash = 'update-profile'; }}><Bell size={17} /><span>Verify contact info<small>{profile?.profileStatus.isEmailVerified && profile.profileStatus.isMobileVerified ? 'Complete' : 'Pending verification'}</small></span><ChevronRight size={17} /></button></div></main>; }
function SimpleTab({ icon, title, copy, action }: { icon: ReactElement; title: string; copy: string; action: string }): ReactElement { return <main className="mobile-content empty-tab"><div className="empty-tab-icon">{icon}</div><p className="mobile-kicker">Kirayee workspace</p><h1>{title}</h1><p>{copy}</p><button className="dashboard-primary-button" type="button">{action}<ChevronRight size={17} /></button></main>; }
function OverviewCard({ icon, label, value, tone }: { icon: ReactElement; label: string; value: string; tone: string }): ReactElement { return <article className={`overview-card ${tone}`}><div>{icon}</div><span>{label}</span><strong>{value}</strong></article>; }
function ProfileRow({ label, value }: { label: string; value: string }): ReactElement { return <div className="profile-row"><span>{label}</span><strong>{value}</strong></div>; }
function NavButton({ active, icon, label, onClick }: { active: boolean; icon: ReactElement; label: string; onClick: () => void }): ReactElement { return <button className={`nav-button ${active ? 'active' : ''}`} type="button" onClick={onClick}>{icon}<span>{label}</span></button>; }
