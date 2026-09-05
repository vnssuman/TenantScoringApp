import { useState, type FormEvent, type ReactElement } from 'react';
import { AuthApiError, loginUser, resetPin } from '../../services/authApi';
import type { AuthFieldErrors, LoginRequest, ResetPinRequest } from '../../types/auth';

interface LoginUserPageProps {
  onRegister: () => void;
  onLoggedIn: (firstName: string, tenantId: string) => void;
}

const initialLogin: LoginRequest = { countryCode: '+91', mobileNumber: '', pin: '' };

export function LoginUserPage({ onRegister, onLoggedIn }: LoginUserPageProps): ReactElement {
  const [login, setLogin] = useState<LoginRequest>(initialLogin);
  const [errors, setErrors] = useState<AuthFieldErrors>({});
  const [serverError, setServerError] = useState('');
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [showPin, setShowPin] = useState(false);
  const [showForgotPin, setShowForgotPin] = useState(false);

  function updateLogin(name: keyof LoginRequest, value: string): void {
    setLogin((current) => ({ ...current, [name]: value }));
    setErrors((current) => ({ ...current, [name]: undefined }));
    setServerError('');
  }

  async function handleLogin(event: FormEvent<HTMLFormElement>): Promise<void> {
    event.preventDefault();
    const nextErrors = validateLogin(login);
    setErrors(nextErrors);
    setServerError('');
    if (Object.keys(nextErrors).length > 0) return;

    setIsSubmitting(true);
    try {
      const response = await loginUser(login);
      localStorage.setItem('kirayeeToken', response.token);
      onLoggedIn(response.firstName, response.tenantId);
    } catch (error) {
      setServerError(error instanceof AuthApiError ? error.message : 'Login could not be completed.');
    } finally {
      setIsSubmitting(false);
    }
  }

  if (showForgotPin) {
    return (
      <ForgotPinForm
        initialCountryCode={login.countryCode}
        initialMobileNumber={login.mobileNumber}
        onBack={() => setShowForgotPin(false)}
      />
    );
  }

  return (
    <section className="login-screen" aria-label="Kirayee login">
      <div className="brand-logo" aria-label="Kirayee">Kirayee</div>
      <p className="login-eyebrow">Welcome back</p>
      <h1>Login to Kirayee</h1>
      <p className="login-copy">Enter your registered mobile number and PIN to continue.</p>
      <form className="login-form" onSubmit={handleLogin} noValidate>
        <CountryCode value={login.countryCode} onChange={(value) => updateLogin('countryCode', value)} error={errors.countryCode} />
        <Field label="Mobile number" id="loginMobileNumber" value={login.mobileNumber} error={errors.mobileNumber} placeholder="Enter 10-digit mobile number" maxLength={10} inputMode="numeric" onChange={(value) => updateLogin('mobileNumber', value)} />
        <div className="field">
          <label htmlFor="loginPin">PIN</label>
          <div className="password-field">
            <input id="loginPin" type={showPin ? 'text' : 'password'} inputMode="numeric" placeholder="Enter your 4-digit PIN" maxLength={4} value={login.pin} onChange={(event) => updateLogin('pin', event.target.value)} aria-invalid={Boolean(errors.pin)} autoComplete="current-password" />
            <button className="visibility-button" type="button" onClick={() => setShowPin((visible) => !visible)} aria-label={showPin ? 'Hide PIN' : 'Show PIN'}>{showPin ? 'Hide' : 'Show'}</button>
          </div>
          {errors.pin && <span className="field-error">{errors.pin}</span>}
        </div>
        {serverError && <div className="server-error" role="alert">{serverError}</div>}
        <button className="primary-button" type="submit" disabled={isSubmitting}>{isSubmitting ? 'Logging in...' : 'Login'}</button>
      </form>
      <button className="text-link" type="button" onClick={() => setShowForgotPin(true)}>Forgot PIN?</button>
      <button className="back-button" type="button" onClick={onRegister}>Register User</button>
    </section>
  );
}

interface ForgotPinFormProps {
  initialCountryCode: string;
  initialMobileNumber: string;
  onBack: () => void;
}

function ForgotPinForm({ initialCountryCode, initialMobileNumber, onBack }: ForgotPinFormProps): ReactElement {
  const [form, setForm] = useState<ResetPinRequest>({ countryCode: initialCountryCode || '+91', mobileNumber: initialMobileNumber, newPin: '', confirmNewPin: '' });
  const [errors, setErrors] = useState<AuthFieldErrors>({});
  const [message, setMessage] = useState('');
  const [isSubmitting, setIsSubmitting] = useState(false);

  function update(name: keyof ResetPinRequest, value: string): void {
    setForm((current) => ({ ...current, [name]: value }));
    setErrors((current) => ({ ...current, [name]: undefined }));
    setMessage('');
  }

  async function handleReset(event: FormEvent<HTMLFormElement>): Promise<void> {
    event.preventDefault();
    const nextErrors = validateReset(form);
    setErrors(nextErrors);
    if (Object.keys(nextErrors).length > 0) return;

    setIsSubmitting(true);
    try {
      const response = await resetPin(form);
      setMessage(response.message);
      setTimeout(onBack, 1200);
    } catch (error) {
      setMessage(error instanceof AuthApiError ? error.message : 'PIN reset could not be completed.');
    } finally {
      setIsSubmitting(false);
    }
  }

  return (
    <section className="login-screen" aria-label="Forgot PIN">
      <div className="brand-logo" aria-label="Kirayee">Kirayee</div>
      <p className="login-eyebrow">Account recovery</p>
      <h1>Reset your PIN</h1>
      <p className="login-copy">Enter your registered mobile number and choose a new PIN.</p>
      <form className="login-form" onSubmit={handleReset} noValidate>
        <CountryCode value={form.countryCode} onChange={(value) => update('countryCode', value)} error={errors.countryCode} />
        <Field label="Mobile number" id="resetMobileNumber" value={form.mobileNumber} error={errors.mobileNumber} placeholder="Enter 10-digit mobile number" maxLength={10} inputMode="numeric" onChange={(value) => update('mobileNumber', value)} />
        <Field label="New PIN" id="newPin" value={form.newPin} error={errors.newPin} placeholder="Enter your 4-digit PIN" maxLength={4} inputMode="numeric" type="password" onChange={(value) => update('newPin', value)} />
        <Field label="Confirm new PIN" id="confirmNewPin" value={form.confirmNewPin} error={errors.confirmNewPin} placeholder="Re-enter your 4-digit PIN" maxLength={4} inputMode="numeric" type="password" onChange={(value) => update('confirmNewPin', value)} />
        {message && <div className="success-message" role="status">{message}</div>}
        <button className="primary-button" type="submit" disabled={isSubmitting}>{isSubmitting ? 'Updating...' : 'Update PIN'}</button>
      </form>
      <button className="back-button" type="button" onClick={onBack}>Back to Login</button>
    </section>
  );
}

function validateLogin(form: LoginRequest): AuthFieldErrors {
  const errors: AuthFieldErrors = {};
  if (!form.countryCode) errors.countryCode = 'Please select a country code.';
  if (!form.mobileNumber) errors.mobileNumber = 'Mobile Number is required.';
  else if (!/^\d{10}$/.test(form.mobileNumber)) errors.mobileNumber = 'Please enter a valid 10-digit mobile number.';
  if (!form.pin) errors.pin = 'PIN is required.';
  else if (!/^\d{4}$/.test(form.pin)) errors.pin = 'PIN must be exactly 4 digits.';
  return errors;
}

function validateReset(form: ResetPinRequest): AuthFieldErrors {
  const errors: AuthFieldErrors = {};
  if (!form.countryCode) errors.countryCode = 'Please select a country code.';
  if (!form.mobileNumber) errors.mobileNumber = 'Mobile Number is required to reset PIN.';
  else if (!/^\d{10}$/.test(form.mobileNumber)) errors.mobileNumber = 'Please enter a valid 10-digit mobile number.';
  if (!form.newPin) errors.newPin = 'New PIN is required.';
  else if (!/^\d{4}$/.test(form.newPin)) errors.newPin = 'PIN must be exactly 4 digits.';
  if (form.confirmNewPin && form.newPin !== form.confirmNewPin) errors.confirmNewPin = 'New PIN and Confirm PIN do not match.';
  return errors;
}

interface FieldProps {
  label: string;
  id: string;
  value: string;
  error?: string;
  placeholder: string;
  maxLength: number;
  inputMode: 'numeric';
  type?: string;
  onChange: (value: string) => void;
}

function Field({ label, id, value, error, placeholder, maxLength, inputMode, type = 'tel', onChange }: FieldProps): ReactElement {
  return <div className="field"><label htmlFor={id}>{label}</label><input id={id} type={type} inputMode={inputMode} placeholder={placeholder} maxLength={maxLength} value={value} onChange={(event) => onChange(event.target.value)} aria-invalid={Boolean(error)} />{error && <span className="field-error">{error}</span>}</div>;
}

interface CountryCodeProps {
  value: string;
  error?: string;
  onChange: (value: string) => void;
}

function CountryCode({ value, error, onChange }: CountryCodeProps): ReactElement {
  return <div className="field"><label htmlFor="authCountryCode">Country code</label><select id="authCountryCode" value={value} onChange={(event) => onChange(event.target.value)} aria-invalid={Boolean(error)}><option value="+91">+91 India</option><option value="+1">+1 United States</option><option value="+44">+44 United Kingdom</option><option value="+61">+61 Australia</option></select>{error && <span className="field-error">{error}</span>}</div>;
}
