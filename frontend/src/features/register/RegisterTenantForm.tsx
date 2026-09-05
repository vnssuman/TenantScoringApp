import { useState, type ChangeEvent, type FormEvent, type ReactElement } from 'react';
import { registerTenant, TenantRegistrationError } from '../../services/tenantService';
import type {
  RegisterTenantFieldErrors,
  RegisterTenantRequest,
  RegisterTenantResponse,
} from '../../types/tenant';

const initialForm: RegisterTenantRequest = {
  countryCode: '+91',
  firstName: '',
  lastName: '',
  mobileNumber: '',
  email: '',
  pin: '',
  confirmPin: '',
};

const initialErrors: RegisterTenantFieldErrors = {};

function validateForm(form: RegisterTenantRequest): RegisterTenantFieldErrors {
  const errors: RegisterTenantFieldErrors = {};
  const namePattern = /^[\p{L} -]+$/u;
  const digitsOnly = /^\d+$/;

  if (!form.countryCode.trim()) errors.countryCode = 'Please select a country code.';
  if (!form.firstName.trim()) errors.firstName = 'First Name is required.';
  else if (!namePattern.test(form.firstName)) errors.firstName = 'First Name should contain only letters.';
  if (!form.lastName.trim()) errors.lastName = 'Last Name is required.';
  else if (!namePattern.test(form.lastName)) errors.lastName = 'Last Name should contain only letters.';
  if (!form.mobileNumber.trim()) errors.mobileNumber = 'Mobile Number is required.';
  else if (form.mobileNumber.length !== 10 || !digitsOnly.test(form.mobileNumber)) {
    errors.mobileNumber = 'Please enter a valid 10-digit mobile number.';
  }
  if (!form.email.trim()) errors.email = 'Email is required.';
  else if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(form.email)) {
    errors.email = 'Please enter a valid email address.';
  }
  if (!form.pin) errors.pin = 'PIN is required.';
  else if (form.pin.length !== 4 || !digitsOnly.test(form.pin)) errors.pin = 'PIN must be exactly 4 digits.';
  if (!form.confirmPin) errors.confirmPin = 'Confirm PIN is required.';
  else if (form.pin !== form.confirmPin) errors.confirmPin = 'PIN and Confirm PIN do not match.';

  return errors;
}

export function RegisterTenantForm(): ReactElement {
  const [form, setForm] = useState<RegisterTenantRequest>(initialForm);
  const [errors, setErrors] = useState<RegisterTenantFieldErrors>(initialErrors);
  const [serverError, setServerError] = useState('');
  const [result, setResult] = useState<RegisterTenantResponse | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);

  function handleChange(event: ChangeEvent<HTMLInputElement | HTMLSelectElement>): void {
    const { name, value } = event.target;
    setForm((current) => ({ ...current, [name]: value }));
    setErrors((current) => ({ ...current, [name]: undefined }));
    setServerError('');
  }

  async function handleSubmit(event: FormEvent<HTMLFormElement>): Promise<void> {
    event.preventDefault();
    const nextErrors = validateForm(form);
    setErrors(nextErrors);
    setServerError('');

    if (Object.keys(nextErrors).length > 0) return;

    setIsSubmitting(true);
    try {
      setResult(await registerTenant(form));
    } catch (error) {
      setServerError(error instanceof TenantRegistrationError ? error.message : 'Registration could not be completed.');
    } finally {
      setIsSubmitting(false);
    }
  }

  if (result) {
    return (
      <section className="success-panel" aria-live="polite">
        <div className="success-mark" aria-hidden="true">OK</div>
        <p className="eyebrow">Registration complete</p>
        <h2>Welcome to Kirayee.</h2>
        <p className="success-copy">Your Kirayee application has been securely registered.</p>
        <div className="tenant-id">
          <span>Your Kirayee ID</span>
          <strong>{result.tenantId}</strong>
        </div>
        <button className="secondary-button" type="button" onClick={() => {
          window.location.hash = 'login';
        }}>
          Click Login
        </button>
      </section>
    );
  }

  return (
    <form className="registration-form" onSubmit={handleSubmit} noValidate>
      <div className="form-heading">
        <p>Enter your details to create your Kirayee profile.</p>
      </div>

      <div className="form-grid">
        <Field label="First name" name="firstName" value={form.firstName} error={errors.firstName} onChange={handleChange} autoComplete="given-name" />
        <Field label="Last name" name="lastName" value={form.lastName} error={errors.lastName} onChange={handleChange} autoComplete="family-name" />
        <div className="field">
          <label htmlFor="countryCode">Country code</label>
          <select id="countryCode" name="countryCode" value={form.countryCode} onChange={handleChange} aria-invalid={Boolean(errors.countryCode)}>
            <option value="+1">+1 United States</option>
            <option value="+44">+44 United Kingdom</option>
            <option value="+61">+61 Australia</option>
            <option value="+91">+91 India</option>
          </select>
          {errors.countryCode && <span className="field-error">{errors.countryCode}</span>}
        </div>
        <Field label="Mobile number" name="mobileNumber" value={form.mobileNumber} error={errors.mobileNumber} onChange={handleChange} inputMode="numeric" autoComplete="tel" maxLength={10} />
        <Field label="Email address" name="email" type="email" value={form.email} error={errors.email} onChange={handleChange} autoComplete="email" />
        <div className="field field-full">
          <label htmlFor="pin">Create a 4-digit PIN</label>
          <input id="pin" name="pin" type="password" inputMode="numeric" maxLength={4} value={form.pin} onChange={handleChange} aria-invalid={Boolean(errors.pin)} autoComplete="new-password" />
          {errors.pin && <span className="field-error">{errors.pin}</span>}
        </div>
        <div className="field field-full">
          <label htmlFor="confirmPin">Confirm your PIN</label>
          <input id="confirmPin" name="confirmPin" type="password" inputMode="numeric" maxLength={4} value={form.confirmPin} onChange={handleChange} aria-invalid={Boolean(errors.confirmPin)} autoComplete="new-password" />
          {errors.confirmPin && <span className="field-error">{errors.confirmPin}</span>}
        </div>
      </div>

      {serverError && <div className="server-error" role="alert">{serverError}</div>}
      <button className="primary-button" type="submit" disabled={isSubmitting}>
        {isSubmitting ? 'Submitting...' : 'Submit'}
      </button>
      <p className="privacy-note">Your PIN is encrypted before it is stored.</p>
      <button className="back-button register-login-link" type="button" onClick={() => {
        window.location.hash = 'login';
      }}>
        Back to login
      </button>
    </form>
  );
}

interface FieldProps {
  label: string;
  name: keyof RegisterTenantRequest;
  value: string;
  error?: string;
  type?: string;
  inputMode?: 'text' | 'numeric' | 'email' | 'tel' | 'url' | 'search' | 'decimal' | 'none';
  autoComplete?: string;
  maxLength?: number;
  onChange: (event: ChangeEvent<HTMLInputElement>) => void;
}

function Field({ label, name, value, error, type = 'text', inputMode, autoComplete, maxLength, onChange }: FieldProps): ReactElement {
  return (
    <div className="field">
      <label htmlFor={name}>{label}</label>
      <input id={name} name={name} type={type} value={value} onChange={onChange} inputMode={inputMode} autoComplete={autoComplete} maxLength={maxLength} aria-invalid={Boolean(error)} />
      {error && <span className="field-error">{error}</span>}
    </div>
  );
}
