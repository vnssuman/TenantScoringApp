import { useEffect, useState, type ReactElement } from 'react';
import { RegisterTenantForm } from './features/register/RegisterTenantForm';
import { LoginScreen } from './features/login/LoginScreen';
import './styles.css';

export default function App(): ReactElement {
	const [isLoginRoute, setIsLoginRoute] = useState(() => window.location.hash === '#login');

	useEffect(() => {
		const handleHashChange = (): void => setIsLoginRoute(window.location.hash === '#login');
		window.addEventListener('hashchange', handleHashChange);
		return () => window.removeEventListener('hashchange', handleHashChange);
	}, []);

	return (
		<main className="app-shell">
			<section className="registration-card" aria-label="Kirayee registration">
				{isLoginRoute ? <LoginScreen /> : <>
					<div className="brand-logo" aria-label="Kirayee">Kirayee</div>
					<div className="progress-track" aria-label="Registration progress"><span /></div>
					<RegisterTenantForm />
				</>}
			</section>
		</main>
	);
}
