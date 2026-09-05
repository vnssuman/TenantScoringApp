import { useState, type FormEvent, type ReactElement } from 'react';

export function LoginScreen(): ReactElement {
  const [isSubmitting, setIsSubmitting] = useState(false);

  function handleSubmit(event: FormEvent<HTMLFormElement>): void {
    event.preventDefault();
    setIsSubmitting(true);
  }

  return (
    <section className="login-screen" aria-label="Kirayee login">
      <div className="brand-logo" aria-label="Kirayee">Kirayee</div>
      <p className="login-eyebrow">Welcome back</p>
      <h1>Login to Kirayee</h1>
      <p className="login-copy">Enter your registered mobile number and PIN to continue.</p>
      <form className="login-form" onSubmit={handleSubmit}>
        <div className="field">
          <label htmlFor="loginMobileNumber">Mobile number</label>
          <input id="loginMobileNumber" name="mobileNumber" type="tel" inputMode="numeric" placeholder="Enter 10-digit mobile number" maxLength={10} autoComplete="tel" required />
        </div>
        <div className="field">
          <label htmlFor="loginPin">PIN</label>
          <input id="loginPin" name="pin" type="password" inputMode="numeric" placeholder="Enter your 4-digit PIN" maxLength={4} autoComplete="current-password" required />
        </div>
        <button className="primary-button" type="submit" disabled={isSubmitting}>
          {isSubmitting ? 'Logging in...' : 'Login'}
        </button>
      </form>
      <button className="back-button" type="button" onClick={() => { window.location.hash = ''; }}>
        Back to registration
      </button>
    </section>
  );
}
