import { useEffect, useState, type FormEvent, type ReactElement } from 'react';
import { Check, ChevronLeft, ShieldAlert } from 'lucide-react';
import { getBehavioralAssessment, getCurrentProfile, submitBehavioralAssessment, updateProfile, verifyProfile } from '../../services/profileApi';
import type { ProfileStatus } from '../../types/auth';
import type { BehavioralAnswer, BehavioralAssessment, ProfileRole, ProfileUpdateRequest, UserProfile, VerificationRequest } from '../../types/profile';

interface ProfileCompletionPageProps { onComplete: (profile: UserProfile) => void; onBack: () => void; }
const initialForm: ProfileUpdateRequest = { firstName: '', lastName: '', mobileNumber: '', email: '', panNumber: '', aadhaarNumber: '', role: 'Tenant' };

export function ProfileCompletionPage({ onComplete, onBack }: ProfileCompletionPageProps): ReactElement {
  const [profile, setProfile] = useState<UserProfile | null>(null);
  const [form, setForm] = useState<ProfileUpdateRequest>(initialForm);
  const [assessment, setAssessment] = useState<BehavioralAssessment | null>(null);
  const [answers, setAnswers] = useState<BehavioralAnswer[]>([]);
  const [channel, setChannel] = useState<VerificationRequest['channel']>('email');
  const [code, setCode] = useState('');
  const [message, setMessage] = useState('');
  const [error, setError] = useState('');
  const [saving, setSaving] = useState(false);

  async function loadProfile(): Promise<void> {
    try {
      const current = await getCurrentProfile();
      const currentAssessment = await getBehavioralAssessment();
      setProfile(current);
      setForm({ firstName: current.firstName ?? '', lastName: current.lastName ?? '', mobileNumber: current.mobileNumber ?? '', email: current.email ?? '', panNumber: current.panNumber ?? '', aadhaarNumber: current.aadhaarNumber ?? '', role: current.currentRole as ProfileRole });
      setAssessment(currentAssessment);
      setAnswers(currentAssessment.answers ?? []);
    } catch (reason) { setError(reason instanceof Error ? reason.message : 'Profile could not be loaded.'); }
  }

  useEffect(() => { void loadProfile(); }, []);

  async function save(event: FormEvent<HTMLFormElement>): Promise<void> {
    event.preventDefault();
    setError('');
    if (!/^\d{10,15}$/.test(form.mobileNumber.trim()) || !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(form.email.trim()) || !/^[A-Za-z]{5}\d{4}[A-Za-z]$/.test(form.panNumber.trim()) || !/^\d{12}$/.test(form.aadhaarNumber.trim())) {
      setError('Enter a valid mobile number, email, PAN, and 12-digit Aadhaar number before saving.');
      return;
    }
    setSaving(true);
    try { const updated = await updateProfile(form); setProfile(updated); setMessage('Profile details saved.'); onComplete(updated); }
    catch (reason) { setError(reason instanceof Error ? reason.message : 'Profile could not be updated.'); }
    finally { setSaving(false); }
  }

  async function verify(event: FormEvent<HTMLFormElement>): Promise<void> {
    event.preventDefault(); setSaving(true); setError('');
    try { await verifyProfile({ channel, code }); const updated = await getCurrentProfile(); setProfile(updated); setMessage(`${channel} verification completed.`); onComplete(updated); }
    catch (reason) { setError(reason instanceof Error ? reason.message : 'Verification failed.'); }
    finally { setSaving(false); }
  }

  async function submitAssessment(event: FormEvent<HTMLFormElement>): Promise<void> {
    event.preventDefault();
    setError('');
    const selectedAnswers = assessment?.questions.map((question) =>
      answers.find((answer) => answer.questionId === question.questionId),
    );
    if (!selectedAnswers || selectedAnswers.some((answer) => answer === undefined)) {
      setError('Please answer all assessment questions before saving.');
      return;
    }
    const submittedAnswers = selectedAnswers.filter((answer): answer is BehavioralAnswer => answer !== undefined);

    setSaving(true);
    try { const result = await submitBehavioralAssessment(submittedAnswers); setAssessment(result); setAnswers(result.answers ?? submittedAnswers); const updated = await getCurrentProfile(); setProfile(updated); setMessage('Behavioral assessment saved.'); onComplete(updated); }
    catch (reason) { setError(reason instanceof Error ? reason.message : 'Assessment could not be saved.'); }
    finally { setSaving(false); }
  }

  if (error && !profile) return <section className="profile-completion-page"><button className="back-button" type="button" onClick={onBack}><ChevronLeft size={16} /> Back</button><div className="server-error" role="alert">{error}</div></section>;
  if (!profile) return <section className="profile-completion-page"><p className="login-copy">Loading your profile...</p></section>;

  const status: ProfileStatus = profile.profileStatus;
  return (
    <section className="profile-completion-page" aria-label="Complete profile">
      <button className="back-button profile-back" type="button" onClick={onBack}><ChevronLeft size={16} /> Back to home</button>
      <p className="login-eyebrow">Profile setup</p>
      <h1>Complete your profile</h1>
      <p className="login-copy">Enter and save your mobile number, email, PAN, Aadhaar, and role.</p>
      <div className="completion-summary"><strong>{status.completionPercentage}%</strong><span>complete</span><div className="completion-meter"><span style={{ width: `${status.completionPercentage}%` }} /></div></div>
      <form className="profile-form" onSubmit={save}>
        <ProfileField label="First name" value={form.firstName} onChange={(value) => setForm({ ...form, firstName: value })} />
        <ProfileField label="Last name" value={form.lastName} onChange={(value) => setForm({ ...form, lastName: value })} />
        <ProfileField label="Mobile number" value={form.mobileNumber} onChange={(value) => setForm({ ...form, mobileNumber: value })} maxLength={15} inputMode="numeric" />
        <ProfileField label="Email address" value={form.email} onChange={(value) => setForm({ ...form, email: value })} inputMode="email" />
        <ProfileField label="PAN number" value={form.panNumber} onChange={(value) => setForm({ ...form, panNumber: value.toUpperCase() })} maxLength={10} />
        <ProfileField label="Aadhaar number" value={form.aadhaarNumber} onChange={(value) => setForm({ ...form, aadhaarNumber: value })} maxLength={12} inputMode="numeric" />
        <label className="field">Role<select value={form.role} onChange={(event) => setForm({ ...form, role: event.target.value as ProfileRole })}><option value="Owner">Owner</option><option value="Tenant">Tenant</option><option value="Both">Both</option></select></label>
        <button className="primary-button" type="submit" disabled={saving}>{saving ? 'Saving...' : 'Save profile details'}</button>
      </form>
      {assessment && <BehavioralAssessmentForm assessment={assessment} answers={answers} onAnswersChange={setAnswers} onSubmit={submitAssessment} disabled={saving} />}
      <div className="verification-list"><VerificationRow label="Mobile number" verified={status.isMobileVerified} /><VerificationRow label="Email address" verified={status.isEmailVerified} /><VerificationRow label="PAN" verified={status.isPanVerified} /><VerificationRow label="Aadhaar" verified={status.isAadhaarVerified} /></div>
      <form className="verification-form" onSubmit={verify}><label htmlFor="verificationChannel">Verify pending item</label><select id="verificationChannel" value={channel} onChange={(event) => setChannel(event.target.value as VerificationRequest['channel'])}><option value="email">Email</option><option value="mobile">Mobile</option><option value="pan">PAN</option><option value="aadhaar">Aadhaar</option></select><input value={code} onChange={(event) => setCode(event.target.value)} inputMode="numeric" placeholder="Enter OTP" maxLength={6} required /><button className="secondary-button" type="submit" disabled={saving}>Verify with OTP</button></form>
      {message && <div className="success-message" role="status">{message}</div>}{error && <div className="server-error" role="alert">{error}</div>}
    </section>
  );
}

function ProfileField({ label, value, onChange, maxLength, inputMode = 'text' }: { label: string; value: string; onChange: (value: string) => void; maxLength?: number; inputMode?: 'text' | 'numeric' | 'email' }): ReactElement { return <div className="field"><label>{label}</label><input value={value} onChange={(event) => onChange(event.target.value)} maxLength={maxLength} inputMode={inputMode} type={inputMode === 'email' ? 'email' : 'text'} required /></div>; }
function BehavioralAssessmentForm({ assessment, answers = [], onAnswersChange, onSubmit, disabled }: { assessment: BehavioralAssessment; answers?: BehavioralAnswer[]; onAnswersChange: (answers: BehavioralAnswer[]) => void; onSubmit: (event: FormEvent<HTMLFormElement>) => Promise<void>; disabled: boolean }): ReactElement {
  function setAnswer(questionId: string, answer: 'Yes' | 'No'): void { onAnswersChange([...answers.filter(item => item.questionId !== questionId), { questionId, answer }]); }
  return <form className="profile-form assessment-form" onSubmit={onSubmit}><p className="login-eyebrow">Behavioral assessment</p><h2>{assessment.completed ? 'Update your answers' : 'Answer all three questions'}</h2>{assessment.questions.map(question => <fieldset className="assessment-question" key={question.questionId}><legend>{question.questionText}</legend><label><input type="radio" name={question.questionId} checked={answers.find(answer => answer.questionId === question.questionId)?.answer === 'Yes'} onChange={() => setAnswer(question.questionId, 'Yes')} /> Yes</label><label><input type="radio" name={question.questionId} checked={answers.find(answer => answer.questionId === question.questionId)?.answer === 'No'} onChange={() => setAnswer(question.questionId, 'No')} /> No</label></fieldset>)}<button className="primary-button" type="submit" disabled={disabled || answers.length !== assessment.questions.length}>{assessment.completed ? 'Update assessment' : 'Save assessment'}</button></form>;
}
function VerificationRow({ label, verified }: { label: string; verified: boolean }): ReactElement { return <div className={`verification-row ${verified ? 'verified' : 'pending'}`}><span>{verified ? <Check size={16} /> : <ShieldAlert size={16} />}</span><strong>{label}</strong><em>{verified ? 'Verified' : 'Pending'}</em></div>; }
