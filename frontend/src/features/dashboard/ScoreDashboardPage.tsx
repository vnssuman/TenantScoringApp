import { useEffect, useState, type ReactElement } from 'react';
import { Bell, Building2, Camera, ChevronRight, CircleUserRound, CreditCard, Home, LogOut, RefreshCw, ShieldCheck, Sparkles, WalletCards } from 'lucide-react';
import { getDashboardSummary } from '../../services/dashboardApi';
import { getCurrentProfile, getProfileImage, uploadProfileImage } from '../../services/profileApi';
import type { DashboardUserSummary } from '../../types/dashboard';
import type { UserProfile } from '../../types/profile';
import { MapTenantPage } from '../mapping/MapTenantPage';
import { OwnerPropertiesPage } from '../properties/OwnerPropertiesPage';
import { TenantRentalRequestsPage } from '../rentalRequests/TenantRentalRequestsPage';
import { TenantRentalHistoryPage } from '../rentalRequests/TenantRentalHistoryPage';
import { TenantPaymentRecordPage } from '../rentalRequests/TenantPaymentRecordPage';
import { ProfileCompletionPage } from '../profile/ProfileCompletionPage';
import { ExplorerActionsGrid } from './ExplorerActionsGrid';

interface ScoreDashboardPageProps { onLogout: () => void; showProfileCompletion?: boolean; onProfileUpdated?: (profile: UserProfile) => void; showProperties?: boolean; showMapTenant?: boolean; showTenantRequests?: boolean; showTenantHistory?: boolean; showTenantPayments?: boolean; }
type Tab = 'home' | 'explore' | 'activity' | 'profile';
type Role = 'tenant' | 'owner';

export function ScoreDashboardPage({ onLogout, showProfileCompletion = false, onProfileUpdated, showProperties = false, showMapTenant = false, showTenantRequests = false, showTenantHistory = false, showTenantPayments = false }: ScoreDashboardPageProps): ReactElement {
  const [summary, setSummary] = useState<DashboardUserSummary | null>(null);
  const [error, setError] = useState('');
  const [tab, setTab] = useState<Tab>('home');
  const [role, setRole] = useState<Role>('tenant');
  const [profile, setProfile] = useState<UserProfile | null>(null);
  const [profileImageUrl, setProfileImageUrl] = useState<string | null>(null);
  const [isUploadingImage, setIsUploadingImage] = useState(false);
  const [profileImageError, setProfileImageError] = useState('');

  useEffect(() => {
    const token = localStorage.getItem('kirayeeToken'); 
    if (!token) { setError('Your session has expired. Please log in again.'); return; }
    getDashboardSummary(token).then(setSummary).catch((reason: unknown) => setError(reason instanceof Error ? reason.message : 'Dashboard could not be loaded.'));
    getCurrentProfile().then(async (currentProfile) => {
      setProfile(currentProfile);
      if (currentProfile.currentRole.toLowerCase() !== 'both') {
        setRole(currentProfile.currentRole.toLowerCase() === 'owner' ? 'owner' : 'tenant');
      }
      const image = await getProfileImage();
      if (image) setProfileImageUrl(URL.createObjectURL(image));
    }).catch(() => undefined);
  }, []);

  useEffect(() => () => {
    if (profileImageUrl) URL.revokeObjectURL(profileImageUrl);
  }, [profileImageUrl]);

  async function handleProfileImageChange(image: File): Promise<void> {
    if (!['image/png', 'image/jpeg', 'image/webp'].includes(image.type) || image.size > 5 * 1024 * 1024) {
      setProfileImageError('Choose a PNG, JPEG, or WebP image up to 5 MB.');
      return;
    }
    setIsUploadingImage(true);
    setProfileImageError('');
    try {
      await uploadProfileImage(image);
      const savedImage = await getProfileImage();
      if (savedImage) setProfileImageUrl(URL.createObjectURL(savedImage));
    } catch (reason) {
      setProfileImageError(reason instanceof Error ? reason.message : 'Profile image could not be updated.');
    } finally {
      setIsUploadingImage(false);
    }
  }

  function navigateToTab(nextTab: Tab): void {
    setTab(nextTab);
    if (showProperties || showMapTenant || showTenantRequests || showTenantHistory || showTenantPayments || showProfileCompletion) window.location.hash = 'home';
  }

  if (error) return <section className="dashboard-state" role="alert"><ShieldCheck size={34} aria-hidden="true" /><h1>Session unavailable</h1><p>{error}</p><button className="dashboard-primary-button" type="button" onClick={onLogout}>Back to Login</button></section>;
  if (!summary) return <section className="dashboard-state" aria-live="polite"><RefreshCw className="spin" size={30} aria-hidden="true" /><p>Loading your Kirayee space...</p></section>;

  return <section className="mobile-dashboard" aria-label="Kirayee dashboard">
    <header className="mobile-header"><div className="dashboard-brand"><span className="dashboard-brand-mark">K</span><span>Kirayee</span></div><div className="header-actions">{profile && !profile.profileStatus.isProfileComplete && <button className="completion-chip" type="button" onClick={() => { window.location.hash = 'update-profile'; }}>Profile {profile.profileStatus.completionPercentage}%</button>}<button className="icon-button" type="button" aria-label="Notifications"><Bell size={19} /></button><button className="icon-button" type="button" aria-label="Log out" onClick={onLogout}><LogOut size={18} /></button></div></header>
      {showProfileCompletion
        ? <ProfileCompletionPage onComplete={onProfileUpdated ?? (() => { window.location.hash = 'home'; })} onBack={() => { setTab('home'); window.location.hash = 'home'; }} />
        : showTenantPayments
        ? <TenantPaymentRecordPage onBack={() => { setTab('explore'); window.location.hash = 'home'; }} />
        : showTenantHistory
        ? <TenantRentalHistoryPage onBack={() => { setTab('explore'); window.location.hash = 'home'; }} />
        : showTenantRequests
        ? <TenantRentalRequestsPage onBack={() => { setTab('explore'); window.location.hash = 'home'; }} />
        : showMapTenant
        ? <MapTenantPage onBack={() => { setTab('explore'); window.location.hash = 'home'; }} />
        : showProperties
        ? <OwnerPropertiesPage onBack={() => { setTab('explore'); window.location.hash = 'home'; }} />
        : tab === 'home' && <HomeTab summary={summary} role={role} showRoleSwitch={profile?.currentRole.toLowerCase() === 'both'} onRoleChange={setRole} profileId={profile?.profileId} profileImageUrl={profileImageUrl} isUploadingImage={isUploadingImage} profileImageError={profileImageError} onProfileImageChange={handleProfileImageChange} memberSince={profile?.createdAt} />}
      {!showProfileCompletion && !showProperties && !showMapTenant && !showTenantRequests && !showTenantHistory && !showTenantPayments && tab === 'explore' && (profile
        ? <ExplorerActionsGrid role={profile.currentRole.toLowerCase() === 'both' ? 'both' : role} />
        : <section className="dashboard-state" aria-live="polite"><RefreshCw className="spin" size={24} aria-hidden="true" /><p>Loading your profile...</p></section>)}
    {!showProfileCompletion && !showProperties && !showMapTenant && !showTenantRequests && !showTenantHistory && !showTenantPayments && tab === 'activity' && <SimpleTab icon={<WalletCards size={26} />} title="Your activity" copy="Payments, proposals, and lease updates will appear here." action="View payment history" />}
    {!showProfileCompletion && !showProperties && !showMapTenant && !showTenantRequests && !showTenantHistory && !showTenantPayments && tab === 'profile' && <ProfileTab summary={summary} profile={profile} />}
    <nav className="bottom-nav" aria-label="Main navigation"><NavButton active={!showProfileCompletion && !showProperties && !showMapTenant && !showTenantRequests && !showTenantHistory && !showTenantPayments && tab === 'home'} icon={<Home size={20} />} label="Home" onClick={() => navigateToTab('home')} /><NavButton active={showProperties || showMapTenant || showTenantRequests || showTenantHistory || showTenantPayments || (!showProfileCompletion && tab === 'explore')} icon={<Building2 size={20} />} label="Services" onClick={() => navigateToTab('explore')} /><NavButton active={!showProfileCompletion && !showProperties && !showMapTenant && !showTenantRequests && !showTenantHistory && !showTenantPayments && tab === 'activity'} icon={<CreditCard size={20} />} label="Activity" onClick={() => navigateToTab('activity')} /><NavButton active={!showProfileCompletion && !showProperties && !showMapTenant && !showTenantRequests && !showTenantHistory && !showTenantPayments && tab === 'profile'} icon={<CircleUserRound size={20} />} label="Profile" onClick={() => navigateToTab('profile')} /></nav>
  </section>;
}

function HomeTab({ summary, role, showRoleSwitch, onRoleChange, profileId, profileImageUrl, isUploadingImage, profileImageError, onProfileImageChange, memberSince }: { summary: DashboardUserSummary; role: Role; showRoleSwitch: boolean; onRoleChange: (role: Role) => void; profileId?: string; profileImageUrl: string | null; isUploadingImage: boolean; profileImageError: string; onProfileImageChange: (image: File) => void; memberSince?: string }): ReactElement {
  return <main className="mobile-content"><div className="welcome-row"><h1>Welcome</h1></div><section className="profile-identity-card" aria-label="Kirayee profile identity"><div className="identity-card-top"><span className="identity-card-brand"><span className="dashboard-brand-mark">K</span> Kirayee</span><span className="identity-role">{role === 'owner' ? 'OWNER' : 'TENANT'}</span></div><div className="identity-card-bottom"><div className="identity-photo-wrap">{profileImageUrl ? <img className="identity-photo" src={profileImageUrl} alt={`${summary.fullName} profile`} /> : <div className="identity-photo identity-photo-placeholder" aria-label="No profile image">{summary.fullName.charAt(0)}</div>}<label className={`identity-photo-update ${isUploadingImage ? 'uploading' : ''}`} title="Update profile image"><Camera size={14} /><span>{isUploadingImage ? 'Updating...' : 'Update image'}</span><input type="file" accept="image/png,image/jpeg,image/webp" disabled={isUploadingImage} onChange={(event) => { const image = event.target.files?.[0]; if (image) onProfileImageChange(image); event.target.value = ''; }} /></label></div><div className="identity-details"><div className="identity-member-name"><span>Member Name</span><strong title={summary.fullName}>{summary.fullName}</strong></div><div className="identity-meta-row"><div className="identity-card-id"><span>Kirayee ID</span><strong>{profileId ? formatProfileId(profileId) : 'Loading...'}</strong></div><div className="identity-card-id"><span>Member Since</span><strong>{memberSince ? new Intl.DateTimeFormat('en-US', { year: 'numeric', month: 'short' }).format(new Date(memberSince)) : 'Loading...'}</strong></div></div></div></div></section>{profileImageError && <p className="profile-image-error" role="alert">{profileImageError}</p>}{showRoleSwitch && <div className="role-switch" role="group" aria-label="Active role"><button className={role === 'tenant' ? 'active' : ''} type="button" onClick={() => onRoleChange('tenant')}>I’m a tenant</button><button className={role === 'owner' ? 'active' : ''} type="button" onClick={() => onRoleChange('owner')}>I’m an owner</button></div>}<div className="section-heading mobile-section-heading"><div><p className="mobile-kicker">Overview</p><h2>{role === 'tenant' ? 'Your rental readiness' : 'Your rental space'}</h2></div><ChevronRight size={18} /></div><div className="overview-grid"><OverviewCard icon={<ShieldCheck size={18} />} label="Overall Score" value={summary.overallScore === undefined ? 'Not available' : `${summary.overallScore}/100`} tone="mint" /><OverviewCard icon={<Sparkles size={18} />} label="Rewards" value="120 pts" tone="yellow" /></div><section className="next-step"><div className="next-step-icon"><Sparkles size={18} /></div><div><p className="mobile-kicker">Next step</p><strong>Complete your identity checks</strong><span>Verify PAN and Aadhaar to unlock all features.</span></div><ChevronRight size={18} /></section></main>;
}

function formatProfileId(profileId: string): string {
  return `TNT${profileId.replaceAll('-', '').slice(0, 8).toUpperCase()}`;
}

function ProfileTab({ summary, profile }: { summary: DashboardUserSummary; profile: UserProfile | null }): ReactElement { return <main className="mobile-content"><p className="mobile-kicker">Account</p><h1 className="tab-title">Your profile</h1><section className="profile-intro"><div className="large-avatar">{summary.fullName.charAt(0)}</div><div><h2>{summary.fullName}</h2><p>{summary.email}</p></div></section><div className="profile-list"><ProfileRow label="Kirayee ID" value={summary.tenantId} /><ProfileRow label="Mobile number" value={summary.mobileNumber} /><ProfileRow label="Roles" value={summary.assignedRoles.join(' / ')} /></div><div className="settings-links"><button type="button" onClick={() => { window.location.hash = 'update-profile'; }}><ShieldCheck size={17} /><span>Update identity documents<small>{profile?.profileStatus.isPanVerified && profile.profileStatus.isAadhaarVerified ? 'Complete' : 'Pending verification'}</small></span><ChevronRight size={17} /></button><button type="button" onClick={() => { window.location.hash = 'update-profile'; }}><Bell size={17} /><span>Verify contact info<small>{profile?.profileStatus.isEmailVerified && profile.profileStatus.isMobileVerified ? 'Complete' : 'Pending verification'}</small></span><ChevronRight size={17} /></button></div></main>; }
function SimpleTab({ icon, title, copy, action }: { icon: ReactElement; title: string; copy: string; action: string }): ReactElement { return <main className="mobile-content empty-tab"><div className="empty-tab-icon">{icon}</div><p className="mobile-kicker">Kirayee workspace</p><h1>{title}</h1><p>{copy}</p><button className="dashboard-primary-button" type="button">{action}<ChevronRight size={17} /></button></main>; }
function OverviewCard({ icon, label, value, tone }: { icon: ReactElement; label: string; value: string; tone: string }): ReactElement { return <article className={`overview-card ${tone}`}><div>{icon}</div><span>{label}</span><strong>{value}</strong></article>; }
function ProfileRow({ label, value }: { label: string; value: string }): ReactElement { return <div className="profile-row"><span>{label}</span><strong>{value}</strong></div>; }
function NavButton({ active, icon, label, onClick }: { active: boolean; icon: ReactElement; label: string; onClick: () => void }): ReactElement { return <button className={`nav-button ${active ? 'active' : ''}`} type="button" onClick={onClick}>{icon}<span>{label}</span></button>; }
