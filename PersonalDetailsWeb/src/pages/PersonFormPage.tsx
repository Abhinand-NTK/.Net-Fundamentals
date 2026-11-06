import { useState, type FormEvent, type ReactNode } from 'react';
import { Link, useNavigate, useParams } from 'react-router';
import { ApiError } from '../api/client';
import { personsApi } from '../api/persons';
import {
  ADDRESS_TYPES,
  BLOOD_GROUPS,
  DOCUMENT_TYPES,
  EDUCATION_LEVELS,
  EMPLOYMENT_TYPES,
  GENDERS,
  MARITAL_STATUSES,
  RELATIONSHIPS,
  type AddressType,
  type BloodGroup,
  type DocumentType,
  type EducationLevel,
  type EmploymentType,
  type Gender,
  type MaritalStatus,
  type PersonDetailResponse,
  type PersonRequest,
  type Relationship,
} from '../api/types';
import { ErrorMessage } from '../components/ErrorMessage';
import { Icon, type IconName } from '../components/Icon';
import { useToast } from '../components/Toast';
import { useApi } from '../hooks/useApi';
import { label } from '../utils/format';

// ---------------------------------------------------------------------------------------------
// Form model. HTML inputs always hold strings, so the form keeps strings and converts to the
// API's PersonRequest shape (numbers, nulls, arrays) only when submitting.
// ---------------------------------------------------------------------------------------------
interface AddressForm { type: AddressType; line1: string; line2: string; city: string; state: string; postalCode: string; country: string; isPrimary: boolean }
interface ContactForm { name: string; relationship: Relationship; phone: string; email: string }
interface EducationForm { level: EducationLevel; institution: string; fieldOfStudy: string; startYear: string; endYear: string; grade: string }
interface EmploymentForm { employer: string; jobTitle: string; employmentType: EmploymentType; startDate: string; endDate: string }
interface DocumentForm { documentType: DocumentType; documentNumber: string; maskedNumber?: string; issuingCountry: string; issueDate: string; expiryDate: string }

interface PersonForm {
  firstName: string; middleName: string; lastName: string; preferredName: string;
  dateOfBirth: string; placeOfBirth: string;
  gender: Gender; maritalStatus: MaritalStatus; bloodGroup: BloodGroup; nationality: string;
  email: string; primaryPhone: string; alternatePhone: string;
  languages: string; notes: string;
  addresses: AddressForm[]; emergencyContacts: ContactForm[]; educations: EducationForm[];
  employments: EmploymentForm[]; identityDocuments: DocumentForm[];
}

const blank = {
  address: (): AddressForm => ({ type: 'Home', line1: '', line2: '', city: '', state: '', postalCode: '', country: '', isPrimary: false }),
  contact: (): ContactForm => ({ name: '', relationship: 'Spouse', phone: '', email: '' }),
  education: (): EducationForm => ({ level: 'Bachelor', institution: '', fieldOfStudy: '', startYear: '', endYear: '', grade: '' }),
  employment: (): EmploymentForm => ({ employer: '', jobTitle: '', employmentType: 'FullTime', startDate: '', endDate: '' }),
  document: (): DocumentForm => ({ documentType: 'Passport', documentNumber: '', issuingCountry: '', issueDate: '', expiryDate: '' }),
};

const emptyForm = (): PersonForm => ({
  firstName: '', middleName: '', lastName: '', preferredName: '', dateOfBirth: '', placeOfBirth: '',
  gender: 'Male', maritalStatus: 'Single', bloodGroup: 'Unknown', nationality: '',
  email: '', primaryPhone: '', alternatePhone: '', languages: '', notes: '',
  addresses: [], emergencyContacts: [], educations: [], employments: [], identityDocuments: [],
});

/** Response → form. Identity document numbers come back masked, so they must be re-entered. */
function fromResponse(p: PersonDetailResponse): PersonForm {
  const s = (v: string | number | undefined) => (v === undefined ? '' : String(v));
  return {
    firstName: p.firstName, middleName: s(p.middleName), lastName: p.lastName, preferredName: s(p.preferredName),
    dateOfBirth: p.dateOfBirth, placeOfBirth: s(p.placeOfBirth),
    gender: p.gender, maritalStatus: p.maritalStatus, bloodGroup: p.bloodGroup, nationality: p.nationality,
    email: p.email, primaryPhone: p.primaryPhone, alternatePhone: s(p.alternatePhone),
    languages: p.languages.join(', '), notes: s(p.notes),
    addresses: p.addresses.map(a => ({ type: a.type, line1: a.line1, line2: s(a.line2), city: a.city, state: s(a.state), postalCode: a.postalCode, country: a.country, isPrimary: a.isPrimary })),
    emergencyContacts: p.emergencyContacts.map(c => ({ name: c.name, relationship: c.relationship, phone: c.phone, email: s(c.email) })),
    educations: p.educations.map(e => ({ level: e.level, institution: e.institution, fieldOfStudy: s(e.fieldOfStudy), startYear: s(e.startYear), endYear: s(e.endYear), grade: s(e.grade) })),
    employments: p.employments.map(e => ({ employer: e.employer, jobTitle: e.jobTitle, employmentType: e.employmentType, startDate: e.startDate, endDate: s(e.endDate) })),
    identityDocuments: p.identityDocuments.map(d => ({ documentType: d.documentType, documentNumber: '', maskedNumber: d.maskedDocumentNumber, issuingCountry: d.issuingCountry, issueDate: s(d.issueDate), expiryDate: s(d.expiryDate) })),
  };
}

/** Form → request body. Blank optional strings become null; numeric text becomes numbers. */
function toRequest(f: PersonForm): PersonRequest {
  const opt = (v: string) => (v.trim() === '' ? null : v.trim());
  const num = (v: string) => (v.trim() === '' ? null : Number(v));
  return {
    firstName: f.firstName.trim(), middleName: opt(f.middleName), lastName: f.lastName.trim(), preferredName: opt(f.preferredName),
    dateOfBirth: f.dateOfBirth, placeOfBirth: opt(f.placeOfBirth),
    gender: f.gender, maritalStatus: f.maritalStatus, bloodGroup: f.bloodGroup, nationality: f.nationality.trim(),
    email: f.email.trim(), primaryPhone: f.primaryPhone.trim(), alternatePhone: opt(f.alternatePhone),
    languages: f.languages.split(',').map(l => l.trim()).filter(Boolean),
    notes: opt(f.notes),
    addresses: f.addresses.map(a => ({ ...a, line2: opt(a.line2), state: opt(a.state) })),
    emergencyContacts: f.emergencyContacts.map(c => ({ ...c, email: opt(c.email) })),
    educations: f.educations.map(e => ({ ...e, fieldOfStudy: opt(e.fieldOfStudy), startYear: Number(e.startYear), endYear: num(e.endYear), grade: opt(e.grade) })),
    employments: f.employments.map(e => ({ ...e, endDate: opt(e.endDate) })),
    identityDocuments: f.identityDocuments.map(({ maskedNumber: _masked, ...d }) => ({ ...d, issueDate: opt(d.issueDate), expiryDate: opt(d.expiryDate) })),
  };
}

// ---------------------------------------------------------------------------------------------
// Validation errors. ASP.NET Core keys them by C# property path ("Addresses[0].City"), or by
// JSON path ("$.dateOfBirth") when the body could not be read. Normalising both to lower case
// lets them match the camelCase paths used below ("addresses[0].city").
// ---------------------------------------------------------------------------------------------
type Errors = Record<string, string[]>;
const normalise = (key: string) => key.replace(/^\$\./, '').toLowerCase();

function Field({ name, path, errors, wide, children }: { name: string; path?: string; errors: Errors; wide?: boolean; children: ReactNode }) {
  const messages = path ? errors[path.toLowerCase()] : undefined;
  return (
    <label className={`field${wide ? ' wide' : ''}${messages ? ' invalid' : ''}`}>
      <span>{name}</span>
      {children}
      {messages?.map(m => <small key={m} className="error">{m}</small>)}
    </label>
  );
}

function Options({ values }: { values: readonly string[] }) {
  return <>{values.map(v => <option key={v} value={v}>{label(v)}</option>)}</>;
}

/** A titled card that groups related inputs. */
function FormSection({ title, icon, hint, count, children }: { title: string; icon: IconName; hint?: string; count?: number; children: ReactNode }) {
  return (
    <fieldset className="card form-section">
      <legend className="sr-only">{title}</legend>
      <div className="form-section-head">
        <span className="section-icon"><Icon name={icon} size={16} /></span>
        <div>
          <h2>{title} {count !== undefined && count > 0 && <span className="count">{count}</span>}</h2>
          {hint && <p className="muted small">{hint}</p>}
        </div>
      </div>
      {children}
    </fieldset>
  );
}

/** One repeatable child collection (addresses, contacts, ...): add, edit and remove rows. */
function ListEditor<T>({ title, itemName, icon, hint, path, items, errors, max, create, onChange, render }: {
  title: string; itemName: string; icon: IconName; hint?: string; path: string; items: T[]; errors: Errors; max: number;
  create: () => T; onChange: (items: T[]) => void;
  render: (item: T, update: (patch: Partial<T>) => void, index: number) => ReactNode;
}) {
  const listErrors = errors[path.toLowerCase()];
  const update = (i: number) => (patch: Partial<T>) => onChange(items.map((it, j) => (j === i ? { ...it, ...patch } : it)));
  return (
    <FormSection title={title} icon={icon} hint={hint} count={items.length}>
      {listErrors?.map(m => <p key={m} className="error">{m}</p>)}
      {items.map((item, i) => (
        <div key={i} className="sub-item">
          <div className="sub-item-head">
            <span className="sub-item-title">{itemName} {i + 1}</span>
            <button type="button" className="icon-btn" onClick={() => onChange(items.filter((_, j) => j !== i))} aria-label={`Remove ${itemName.toLowerCase()} ${i + 1}`} title="Remove">
              <Icon name="trash" size={16} />
            </button>
          </div>
          <div className="form-grid">{render(item, update(i), i)}</div>
        </div>
      ))}
      <button type="button" className="add-btn" disabled={items.length >= max} onClick={() => onChange([...items, create()])}>
        <Icon name="plus" size={16} /> Add {itemName.toLowerCase()}
      </button>
    </FormSection>
  );
}

// ---------------------------------------------------------------------------------------------
// Page: /persons/new and /persons/:id/edit
// ---------------------------------------------------------------------------------------------
export function PersonFormPage() {
  const { id } = useParams();
  if (!id) return <PersonEditor initial={emptyForm()} />;
  return <EditLoader id={id} />;
}

function EditLoader({ id }: { id: string }) {
  const { data, error, loading } = useApi(signal => personsApi.get(id, signal), [id]);
  if (loading) return <div className="page"><p className="muted">Loading…</p></div>;
  if (error || !data) return <div className="page"><ErrorMessage error={error} /></div>;
  return <PersonEditor id={id} initial={fromResponse(data)} />;
}

function PersonEditor({ id, initial }: { id?: string; initial: PersonForm }) {
  const navigate = useNavigate();
  const toast = useToast();
  const [form, setForm] = useState(initial);
  const [errors, setErrors] = useState<Errors>({});
  const [failure, setFailure] = useState<unknown>();
  const [saving, setSaving] = useState(false);

  const patch = (p: Partial<PersonForm>) => setForm(f => ({ ...f, ...p }));
  const text = (key: keyof PersonForm) => ({
    value: form[key] as string,
    onChange: (e: { target: { value: string } }) => patch({ [key]: e.target.value } as Partial<PersonForm>),
  });

  async function handleSubmit(e: FormEvent) {
    e.preventDefault();
    setSaving(true);
    setErrors({});
    setFailure(undefined);
    try {
      const saved = id ? await personsApi.update(id, toRequest(form)) : await personsApi.create(toRequest(form));
      toast(id ? 'Changes saved' : `${saved.fullName} was added`);
      navigate(`/persons/${saved.id}`);
    } catch (err) {
      if (err instanceof ApiError && err.status === 400 && err.problem.errors) {
        setErrors(Object.fromEntries(Object.entries(err.fieldErrors).map(([k, v]) => [normalise(k), v])));
      } else if (err instanceof ApiError && err.status === 409) {
        setErrors({ email: [err.message] }); // duplicate e-mail
      }
      setFailure(err);
      window.scrollTo({ top: 0, behavior: 'smooth' });
    } finally {
      setSaving(false);
    }
  }

  const errorCount = Object.keys(errors).length;

  return (
    <form className="page" onSubmit={handleSubmit} noValidate>
      <Link to={id ? `/persons/${id}` : '/'} className="back"><Icon name="back" size={16} /> Back</Link>
      <header className="page-header">
        <div>
          <h1>{id ? `Edit ${initial.firstName} ${initial.lastName}` : 'Add a person'}</h1>
          <p className="subtitle">Fields marked * are required. Everything is checked by the API when you save.</p>
        </div>
      </header>

      {failure !== undefined && (
        errorCount > 0 ? (
          <div className="alert" role="alert">
            <Icon name="alert" size={18} />
            <div>
              <strong>Please fix {errorCount === 1 ? 'the highlighted field' : `${errorCount} highlighted fields`}.</strong>
              <ul>
                {Object.entries(errors).flatMap(([k, msgs]) => msgs.map(m => <li key={k + m}><code>{k}</code>: {m}</li>))}
              </ul>
            </div>
          </div>
        ) : (
          <ErrorMessage error={failure} />
        )
      )}

      <FormSection title="Personal information" icon="user">
        <div className="form-grid">
          <Field name="First name *" path="firstName" errors={errors}><input required maxLength={100} {...text('firstName')} /></Field>
          <Field name="Middle name" path="middleName" errors={errors}><input maxLength={100} {...text('middleName')} /></Field>
          <Field name="Last name *" path="lastName" errors={errors}><input required maxLength={100} {...text('lastName')} /></Field>
          <Field name="Preferred name" path="preferredName" errors={errors}><input maxLength={100} {...text('preferredName')} /></Field>
          <Field name="Date of birth *" path="dateOfBirth" errors={errors}><input type="date" required {...text('dateOfBirth')} /></Field>
          <Field name="Place of birth" path="placeOfBirth" errors={errors}><input maxLength={150} {...text('placeOfBirth')} /></Field>
          <Field name="Gender *" path="gender" errors={errors}><select {...text('gender')}><Options values={GENDERS} /></select></Field>
          <Field name="Marital status" path="maritalStatus" errors={errors}><select {...text('maritalStatus')}><Options values={MARITAL_STATUSES} /></select></Field>
          <Field name="Blood group" path="bloodGroup" errors={errors}><select {...text('bloodGroup')}><Options values={BLOOD_GROUPS} /></select></Field>
          <Field name="Nationality *" path="nationality" errors={errors}><input required maxLength={100} {...text('nationality')} /></Field>
        </div>
      </FormSection>

      <FormSection title="Contact" icon="mail">
        <div className="form-grid">
          <Field name="E-mail *" path="email" errors={errors}><input type="email" required {...text('email')} /></Field>
          <Field name="Primary phone *" path="primaryPhone" errors={errors}><input type="tel" required placeholder="+91 98470 00001" {...text('primaryPhone')} /></Field>
          <Field name="Alternate phone" path="alternatePhone" errors={errors}><input type="tel" {...text('alternatePhone')} /></Field>
          <Field name="Languages (comma-separated)" path="languages" errors={errors}><input placeholder="English, Malayalam" {...text('languages')} /></Field>
          <Field name="Notes" path="notes" errors={errors} wide><textarea rows={3} maxLength={2000} {...text('notes')} /></Field>
        </div>
      </FormSection>

      <ListEditor title="Addresses" itemName="Address" icon="pin" hint="Tick one address as primary; it is shown on the profile." path="addresses" items={form.addresses} errors={errors} max={10}
        create={blank.address} onChange={addresses => patch({ addresses })}
        render={(a, set, i) => {
          const p = `addresses[${i}]`;
          return (
            <>
              <Field name="Type" errors={errors}><select value={a.type} onChange={e => set({ type: e.target.value as AddressType })}><Options values={ADDRESS_TYPES} /></select></Field>
              <Field name="Line 1 *" path={`${p}.line1`} errors={errors}><input value={a.line1} onChange={e => set({ line1: e.target.value })} /></Field>
              <Field name="Line 2" path={`${p}.line2`} errors={errors}><input value={a.line2} onChange={e => set({ line2: e.target.value })} /></Field>
              <Field name="City *" path={`${p}.city`} errors={errors}><input value={a.city} onChange={e => set({ city: e.target.value })} /></Field>
              <Field name="State" path={`${p}.state`} errors={errors}><input value={a.state} onChange={e => set({ state: e.target.value })} /></Field>
              <Field name="Postal code *" path={`${p}.postalCode`} errors={errors}><input value={a.postalCode} onChange={e => set({ postalCode: e.target.value })} /></Field>
              <Field name="Country *" path={`${p}.country`} errors={errors}><input value={a.country} onChange={e => set({ country: e.target.value })} /></Field>
              <label className="check">
                <input type="checkbox" checked={a.isPrimary}
                  // Only one primary address is allowed, so ticking one unticks the others.
                  onChange={e => patch({ addresses: form.addresses.map((x, j) => ({ ...x, isPrimary: j === i ? e.target.checked : false })) })} />
                Primary address
              </label>
            </>
          );
        }} />

      <ListEditor title="Emergency contacts" itemName="Contact" icon="shield" hint="People to call in an emergency." path="emergencyContacts" items={form.emergencyContacts} errors={errors} max={10}
        create={blank.contact} onChange={emergencyContacts => patch({ emergencyContacts })}
        render={(c, set, i) => {
          const p = `emergencyContacts[${i}]`;
          return (
            <>
              <Field name="Name *" path={`${p}.name`} errors={errors}><input value={c.name} onChange={e => set({ name: e.target.value })} /></Field>
              <Field name="Relationship" errors={errors}><select value={c.relationship} onChange={e => set({ relationship: e.target.value as Relationship })}><Options values={RELATIONSHIPS} /></select></Field>
              <Field name="Phone *" path={`${p}.phone`} errors={errors}><input type="tel" value={c.phone} onChange={e => set({ phone: e.target.value })} /></Field>
              <Field name="E-mail" path={`${p}.email`} errors={errors}><input type="email" value={c.email} onChange={e => set({ email: e.target.value })} /></Field>
            </>
          );
        }} />

      <ListEditor title="Education" itemName="Education" icon="graduation" path="educations" items={form.educations} errors={errors} max={20}
        create={blank.education} onChange={educations => patch({ educations })}
        render={(ed, set, i) => {
          const p = `educations[${i}]`;
          return (
            <>
              <Field name="Level" errors={errors}><select value={ed.level} onChange={e => set({ level: e.target.value as EducationLevel })}><Options values={EDUCATION_LEVELS} /></select></Field>
              <Field name="Institution *" path={`${p}.institution`} errors={errors}><input value={ed.institution} onChange={e => set({ institution: e.target.value })} /></Field>
              <Field name="Field of study" path={`${p}.fieldOfStudy`} errors={errors}><input value={ed.fieldOfStudy} onChange={e => set({ fieldOfStudy: e.target.value })} /></Field>
              <Field name="Start year *" path={`${p}.startYear`} errors={errors}><input type="number" min={1900} max={2100} value={ed.startYear} onChange={e => set({ startYear: e.target.value })} /></Field>
              <Field name="End year" path={`${p}.endYear`} errors={errors}><input type="number" min={1900} max={2100} value={ed.endYear} onChange={e => set({ endYear: e.target.value })} /></Field>
              <Field name="Grade" path={`${p}.grade`} errors={errors}><input value={ed.grade} onChange={e => set({ grade: e.target.value })} /></Field>
            </>
          );
        }} />

      <ListEditor title="Employment" itemName="Job" icon="briefcase" hint="Leave the end date empty for a current job." path="employments" items={form.employments} errors={errors} max={30}
        create={blank.employment} onChange={employments => patch({ employments })}
        render={(em, set, i) => {
          const p = `employments[${i}]`;
          return (
            <>
              <Field name="Employer *" path={`${p}.employer`} errors={errors}><input value={em.employer} onChange={e => set({ employer: e.target.value })} /></Field>
              <Field name="Job title *" path={`${p}.jobTitle`} errors={errors}><input value={em.jobTitle} onChange={e => set({ jobTitle: e.target.value })} /></Field>
              <Field name="Type" errors={errors}><select value={em.employmentType} onChange={e => set({ employmentType: e.target.value as EmploymentType })}><Options values={EMPLOYMENT_TYPES} /></select></Field>
              <Field name="Start date *" path={`${p}.startDate`} errors={errors}><input type="date" value={em.startDate} onChange={e => set({ startDate: e.target.value })} /></Field>
              <Field name="End date (empty = current)" path={`${p}.endDate`} errors={errors}><input type="date" value={em.endDate} onChange={e => set({ endDate: e.target.value })} /></Field>
            </>
          );
        }} />

      <ListEditor title="Identity documents" itemName="Document" icon="idcard" hint="Numbers are stored in full but only ever shown masked." path="identityDocuments" items={form.identityDocuments} errors={errors} max={10}
        create={blank.document} onChange={identityDocuments => patch({ identityDocuments })}
        render={(d, set, i) => {
          const p = `identityDocuments[${i}]`;
          return (
            <>
              <Field name="Type" errors={errors}><select value={d.documentType} onChange={e => set({ documentType: e.target.value as DocumentType })}><Options values={DOCUMENT_TYPES} /></select></Field>
              <Field name="Document number *" path={`${p}.documentNumber`} errors={errors}>
                <input value={d.documentNumber} placeholder={d.maskedNumber ? `Re-enter (stored: ${d.maskedNumber})` : 'P1234567'}
                  onChange={e => set({ documentNumber: e.target.value })} />
              </Field>
              <Field name="Issuing country *" path={`${p}.issuingCountry`} errors={errors}><input value={d.issuingCountry} onChange={e => set({ issuingCountry: e.target.value })} /></Field>
              <Field name="Issue date" path={`${p}.issueDate`} errors={errors}><input type="date" value={d.issueDate} onChange={e => set({ issueDate: e.target.value })} /></Field>
              <Field name="Expiry date" path={`${p}.expiryDate`} errors={errors}><input type="date" value={d.expiryDate} onChange={e => set({ expiryDate: e.target.value })} /></Field>
            </>
          );
        }} />

      {id && form.identityDocuments.some(d => d.maskedNumber) && (
        <div className="callout">
          <Icon name="alert" size={16} />
          <p>
            The API never returns full document numbers (they are masked), and saving replaces the whole record,
            so existing document numbers must be typed in again.
          </p>
        </div>
      )}

      {/* Stays visible at the bottom of the window while scrolling through a long form. */}
      <div className="form-actions">
        <Link to={id ? `/persons/${id}` : '/'} className="btn btn-ghost">Cancel</Link>
        <button type="submit" className="btn btn-primary" disabled={saving}>
          <Icon name="check" size={16} /> {saving ? 'Saving…' : id ? 'Save changes' : 'Create person'}
        </button>
      </div>
    </form>
  );
}
