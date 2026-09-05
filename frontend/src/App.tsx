import { useEffect, useState, type ReactElement } from 'react';
import { RegisterTenantForm } from './features/register/RegisterTenantForm';
import { LoginUserPage } from './features/auth/LoginUserPage';
import { HomeScreen } from './features/home/HomeScreen';
import './styles.css';

export default function App(): ReactElement {
	const [route, setRoute] = useState(() => window.location.hash.replace('#', '') || 'register');
	const [user, setUser] = useState(() => ({
		firstName: sessionStorage.getItem('kirayeeFirstName') ?? '',
		tenantId: sessionStorage.getItem('kirayeeTenantId') ?? '',
	}));

	useEffect(() => {
		const handleHashChange = (): void => setRoute(window.location.hash.replace('#', '') || 'register');
		window.addEventListener('hashchange', handleHashChange);
		return () => window.removeEventListener('hashchange', handleHashChange);
	}, []);

	function handleLoggedIn(firstName: string, tenantId: string): void {
		sessionStorage.setItem('kirayeeFirstName', firstName);
		sessionStorage.setItem('kirayeeTenantId', tenantId);
		setUser({ firstName, tenantId });
		window.location.hash = 'home';
	}

	function handleLogout(): void {
		localStorage.removeItem('kirayeeToken');
		sessionStorage.removeItem('kirayeeFirstName');
		sessionStorage.removeItem('kirayeeTenantId');
		setUser({ firstName: '', tenantId: '' });
		window.location.hash = 'login';
	}

	const content = route === 'login'
		? <LoginUserPage onRegister={() => { window.location.hash = ''; }} onLoggedIn={handleLoggedIn} />
		: route === 'home' && user.firstName
			? <HomeScreen firstName={user.firstName} tenantId={user.tenantId} onLogout={handleLogout} />
			: <>
				<div className="brand-logo" aria-label="Kirayee">Kirayee</div>
				<div className="progress-track" aria-label="Registration progress"><span /></div>
				<RegisterTenantForm />
			</>;

	return (
		<main className="app-shell">
			<section className="registration-card" aria-label="Kirayee registration">
				{content}
			</section>
		</main>
	);
}
