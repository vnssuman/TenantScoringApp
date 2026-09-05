import type { ReactElement } from 'react';

interface HomeScreenProps {
  firstName: string;
  tenantId: string;
  onLogout: () => void;
}

export function HomeScreen({ firstName, tenantId, onLogout }: HomeScreenProps): ReactElement {
  return (
    <section className="home-screen" aria-label="Kirayee home">
      <div className="brand-logo" aria-label="Kirayee">Kirayee</div>
      <p className="login-eyebrow">Your account</p>
      <h1>Welcome, {firstName}.</h1>
      <p className="login-copy">You are securely signed in to Kirayee.</p>
      <div className="tenant-id"><span>Your Kirayee ID</span><strong>{tenantId}</strong></div>
      <button className="primary-button" type="button" onClick={onLogout}>Log out</button>
    </section>
  );
}
