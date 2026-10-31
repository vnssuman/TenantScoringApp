import { useEffect, useState, type ReactElement } from 'react';
import { RegisterTenantForm } from './features/register/RegisterTenantForm';
import { LoginUserPage } from './features/auth/LoginUserPage';
import { ScoreDashboardPage } from './features/dashboard/ScoreDashboardPage';
import { ProfileCompletionPage } from './features/profile/ProfileCompletionPage';
import { getCurrentProfile } from './services/profileApi';
import type { LoginResponse, ProfileStatus } from './types/auth';
import type { UserProfile } from './types/profile';
import './styles.css';

export default function App(): ReactElement {
	const [route, setRoute] = useState(() => window.location.hash.replace('#', '') || 'register');
	const [registrationNotice, setRegistrationNotice] = useState(() => sessionStorage.getItem('kirayeeRegistrationNotice') ?? '');
	const [profileStatus, setProfileStatus] = useState<ProfileStatus | null>(() => {
		const saved = sessionStorage.getItem('kirayeeProfileStatus');
		return saved ? JSON.parse(saved) as ProfileStatus : null;
	});

	useEffect(() => {
		const handleHashChange = (): void => setRoute(window.location.hash.replace('#', '') || 'register');
		window.addEventListener('hashchange', handleHashChange);
		return () => window.removeEventListener('hashchange', handleHashChange);
	}, []);

	useEffect(() => {
		if (!localStorage.getItem('kirayeeToken') || !['home', 'update-profile'].includes(route)) return;
		getCurrentProfile()
			.then((profile) => {
				setProfileStatus(profile.profileStatus);
				sessionStorage.setItem('kirayeeProfileStatus', JSON.stringify(profile.profileStatus));
			})
			.catch(() => {
				if (route === 'home') handleLogout();
			});
	}, [route]);

	function handleLoggedIn(response: LoginResponse): void {
		sessionStorage.setItem('kirayeeFirstName', response.firstName ?? '');
		sessionStorage.setItem('kirayeeTenantId', response.userId);
		setProfileStatus(response.profileStatus);
		sessionStorage.setItem('kirayeeProfileStatus', JSON.stringify(response.profileStatus));
		window.location.hash = response.profileStatus.isProfileComplete ? 'home' : 'update-profile';
	}

	function handleProfileUpdated(profile: UserProfile): void {
		setProfileStatus(profile.profileStatus);
		sessionStorage.setItem('kirayeeProfileStatus', JSON.stringify(profile.profileStatus));
		window.location.hash = 'home';
	}

	function handleLogout(): void {
		localStorage.removeItem('kirayeeToken');
		sessionStorage.removeItem('kirayeeFirstName');
		sessionStorage.removeItem('kirayeeTenantId');
		sessionStorage.removeItem('kirayeeFirstName');
		sessionStorage.removeItem('kirayeeTenantId');
		sessionStorage.removeItem('kirayeeProfileStatus');
		window.location.hash = 'login';
	}

	const content = route === 'login'
		? <LoginUserPage onRegister={() => { setRegistrationNotice(sessionStorage.getItem('kirayeeRegistrationNotice') ?? ''); window.location.hash = ''; }} onLoggedIn={handleLoggedIn} />
		: route === 'update-profile' && localStorage.getItem('kirayeeToken')
			? <ProfileCompletionPage onComplete={handleProfileUpdated} onBack={() => { window.location.hash = 'home'; }} />
		: route === 'home' && localStorage.getItem('kirayeeToken')
			? <ScoreDashboardPage onLogout={handleLogout} />
			: <>
				<div className="brand-logo" aria-label="Kirayee">Kirayee</div>
				<div className="progress-track" aria-label="Registration progress"><span /></div>
				<RegisterTenantForm notice={registrationNotice} />
			</>;

	return (
		<main className="app-shell">
			<section className={route === 'home' ? 'dashboard-card' : 'registration-card'} aria-label={route === 'home' ? 'Kirayee dashboard' : route === 'update-profile' ? 'Complete profile' : 'Kirayee registration'}>
				{content}
			</section>
		</main>
	);
}
