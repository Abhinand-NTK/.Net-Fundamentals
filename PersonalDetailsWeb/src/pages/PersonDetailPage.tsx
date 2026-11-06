import { useState, type ReactNode } from 'react';
import { Link, useNavigate, useParams } from 'react-router';
import { personsApi } from '../api/persons';
import { Avatar } from '../components/Avatar';
import { ErrorMessage } from '../components/ErrorMessage';
import { Icon, type IconName } from '../components/Icon';
import { Skeleton } from '../components/Skeleton';
import { useToast } from '../components/Toast';
import { useApi } from '../hooks/useApi';
import { formatDate, formatDateTime, label } from '../utils/format';

function Item({ name, children }: { name: string; children: ReactNode }) {
  if (children === undefined || children === null || children === '') return null;
  return (
    <div className="item">
      <dt>{name}</dt>
      <dd>{children}</dd>
    </div>
  );
}

function Section({ title, icon, count, empty, children }: { title: string; icon: IconName; count?: number; empty?: string; children: ReactNode }) {
  return (
    <section className="card section">
      <h2 className="section-title">
        <span className="section-icon"><Icon name={icon} size={16} /></span>
        {title}
        {count !== undefined && count > 0 && <span className="count">{count}</span>}
      </h2>
      {count === 0 ? <p className="empty-line">{empty ?? 'None recorded.'}</p> : children}
    </section>
  );
}

function Fact({ icon, name, value }: { icon: IconName; name: string; value: ReactNode }) {
  return (
    <div className="fact">
      <span className="fact-icon"><Icon name={icon} size={16} /></span>
      <div>
        <div className="fact-name">{name}</div>
        <div className="fact-value">{value}</div>
      </div>
    </div>
  );
}

function DetailSkeleton() {
  return (
    <div className="page">
      <div className="card profile">
        <Skeleton width={88} height={88} radius={999} />
        <div className="skeleton-text" style={{ flex: 1 }}>
          <Skeleton width="35%" height={26} />
          <Skeleton width="50%" />
        </div>
      </div>
      <div className="card"><Skeleton height={120} /></div>
    </div>
  );
}

export function PersonDetailPage() {
  const { id = '' } = useParams();
  const navigate = useNavigate();
  const toast = useToast();
  const { data: person, error, loading } = useApi(signal => personsApi.get(id, signal), [id]);
  const [deleteError, setDeleteError] = useState<unknown>();
  const [deleting, setDeleting] = useState(false);

  async function handleDelete() {
    if (!person || !confirm(`Delete ${person.fullName}? The record is soft-deleted and hidden from the API.`)) return;
    setDeleting(true);
    try {
      await personsApi.remove(person.id);
      toast(`${person.fullName} was deleted`);
      navigate('/', { replace: true });
    } catch (err) {
      setDeleteError(err);
      setDeleting(false);
    }
  }

  if (loading && !person) return <DetailSkeleton />;
  if (error || !person) {
    return (
      <div className="page">
        <Link to="/" className="back"><Icon name="back" size={16} /> People</Link>
        <ErrorMessage error={error} />
      </div>
    );
  }

  const primaryAddress = person.addresses.find(a => a.isPrimary) ?? person.addresses[0];
  const currentJob = person.employments.find(e => e.isCurrent);
  const employments = [...person.employments].sort((a, b) => b.startDate.localeCompare(a.startDate));
  const educations = [...person.educations].sort((a, b) => b.startYear - a.startYear);

  return (
    <div className="page">
      <Link to="/" className="back"><Icon name="back" size={16} /> People</Link>

      <section className="card profile">
        <Avatar name={person.fullName} size="lg" />
        <div className="profile-main">
          <h1>{person.fullName}</h1>
          {person.preferredName && <p className="subtitle">Goes by “{person.preferredName}”</p>}
          <p className="profile-headline">
            {currentJob ? <>{currentJob.jobTitle} at <strong>{currentJob.employer}</strong></> : label(person.maritalStatus)}
            {primaryAddress && <> · <Icon name="pin" size={14} /> {primaryAddress.city}, {primaryAddress.country}</>}
          </p>
          <div className="profile-links">
            <a href={`mailto:${person.email}`} className="link-pill"><Icon name="mail" size={14} />{person.email}</a>
            <a href={`tel:${person.primaryPhone.replace(/\s/g, '')}`} className="link-pill"><Icon name="phone" size={14} />{person.primaryPhone}</a>
          </div>
        </div>
        <div className="profile-actions">
          <Link to={`/persons/${person.id}/edit`} className="btn"><Icon name="edit" size={16} /> Edit</Link>
          <button type="button" className="btn btn-danger" onClick={handleDelete} disabled={deleting}>
            <Icon name="trash" size={16} /> {deleting ? 'Deleting…' : 'Delete'}
          </button>
        </div>
      </section>

      <ErrorMessage error={deleteError} />

      <div className="facts">
        <Fact icon="cake" name="Age" value={`${person.age} years`} />
        <Fact icon="user" name="Gender" value={label(person.gender)} />
        <Fact icon="globe" name="Nationality" value={person.nationality} />
        <Fact icon="droplet" name="Blood group" value={label(person.bloodGroup)} />
      </div>

      <div className="detail-grid">
        <div className="detail-main">
          <Section title="Personal information" icon="user">
            <dl className="grid">
              <Item name="First name">{person.firstName}</Item>
              <Item name="Middle name">{person.middleName}</Item>
              <Item name="Last name">{person.lastName}</Item>
              <Item name="Date of birth">{formatDate(person.dateOfBirth)}</Item>
              <Item name="Place of birth">{person.placeOfBirth}</Item>
              <Item name="Marital status">{label(person.maritalStatus)}</Item>
              <Item name="Alternate phone">{person.alternatePhone}</Item>
              <Item name="Languages">
                {person.languages.length ? <span className="tags">{person.languages.map(l => <span key={l} className="tag">{l}</span>)}</span> : undefined}
              </Item>
            </dl>
            {person.notes && (
              <div className="notes">
                <Icon name="note" size={16} />
                <p>{person.notes}</p>
              </div>
            )}
          </Section>

          <Section title="Employment" icon="briefcase" count={employments.length} empty="No employment history.">
            <ol className="timeline">
              {employments.map(e => (
                <li key={e.id} className={e.isCurrent ? 'current' : undefined}>
                  <div className="timeline-head">
                    <strong>{e.jobTitle}</strong>
                    {e.isCurrent && <span className="tag tag-accent">Current</span>}
                  </div>
                  <div>{e.employer} · {label(e.employmentType)}</div>
                  <div className="muted small">{formatDate(e.startDate)} – {e.endDate ? formatDate(e.endDate) : 'Present'}</div>
                </li>
              ))}
            </ol>
          </Section>

          <Section title="Education" icon="graduation" count={educations.length} empty="No education recorded.">
            <ol className="timeline">
              {educations.map(e => (
                <li key={e.id}>
                  <div className="timeline-head"><strong>{e.institution}</strong></div>
                  <div>{label(e.level)}{e.fieldOfStudy && ` · ${e.fieldOfStudy}`}</div>
                  <div className="muted small">
                    {e.startYear} – {e.endYear ?? 'Present'}{e.grade && ` · Grade ${e.grade}`}
                  </div>
                </li>
              ))}
            </ol>
          </Section>
        </div>

        <div className="detail-side">
          <Section title="Addresses" icon="pin" count={person.addresses.length} empty="No addresses.">
            <ul className="stack">
              {person.addresses.map(a => (
                <li key={a.id}>
                  <div className="timeline-head">
                    <strong>{label(a.type)}</strong>
                    {a.isPrimary && <span className="tag tag-accent">Primary</span>}
                  </div>
                  <address>
                    {a.line1}
                    {a.line2 && <><br />{a.line2}</>}
                    <br />
                    {a.city}{a.state && `, ${a.state}`} {a.postalCode}
                    <br />
                    {a.country}
                  </address>
                </li>
              ))}
            </ul>
          </Section>

          <Section title="Emergency contacts" icon="shield" count={person.emergencyContacts.length} empty="No emergency contacts.">
            <ul className="stack">
              {person.emergencyContacts.map(c => (
                <li key={c.id} className="contact">
                  <Avatar name={c.name} size="sm" />
                  <div>
                    <strong>{c.name}</strong> <span className="muted small">· {label(c.relationship)}</span>
                    <div className="small"><a href={`tel:${c.phone.replace(/\s/g, '')}`}>{c.phone}</a></div>
                    {c.email && <div className="small"><a href={`mailto:${c.email}`}>{c.email}</a></div>}
                  </div>
                </li>
              ))}
            </ul>
          </Section>

          <Section title="Identity documents" icon="idcard" count={person.identityDocuments.length} empty="No documents.">
            <ul className="stack">
              {person.identityDocuments.map(d => (
                <li key={d.id}>
                  <div className="timeline-head">
                    <strong>{label(d.documentType)}</strong>
                    {d.isExpired
                      ? <span className="tag tag-warn"><Icon name="alert" size={12} /> Expired</span>
                      : <span className="tag tag-ok"><Icon name="check" size={12} /> Valid</span>}
                  </div>
                  <div className="mono">{d.maskedDocumentNumber}</div>
                  <div className="muted small">
                    {d.issuingCountry}
                    {d.expiryDate && ` · expires ${formatDate(d.expiryDate)}`}
                  </div>
                </li>
              ))}
            </ul>
          </Section>
        </div>
      </div>

      <p className="meta">
        Created {formatDateTime(person.createdAtUtc)}
        {person.updatedAtUtc && ` · Last updated ${formatDateTime(person.updatedAtUtc)}`}
      </p>
    </div>
  );
}
