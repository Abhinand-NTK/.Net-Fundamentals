import { useState, type FormEvent } from 'react';
import { Link, useNavigate, useSearchParams } from 'react-router';
import { personsApi } from '../api/persons';
import {
  BLOOD_GROUPS,
  GENDERS,
  MARITAL_STATUSES,
  SORT_FIELDS,
  type PersonQueryParameters,
} from '../api/types';
import { Avatar } from '../components/Avatar';
import { ErrorMessage } from '../components/ErrorMessage';
import { Icon } from '../components/Icon';
import { Pagination } from '../components/Pagination';
import { SkeletonRows } from '../components/Skeleton';
import { useApi } from '../hooks/useApi';
import { label } from '../utils/format';

// Filter keys are stored in the URL (?search=...&gender=...), so a filtered list can be
// bookmarked or shared, and the browser's back button restores the previous filters.
const FILTER_KEYS = ['search', 'gender', 'bloodGroup', 'maritalStatus', 'city', 'minAge', 'maxAge', 'sortBy', 'sortDirection'] as const;
type FilterKey = (typeof FILTER_KEYS)[number];
type Filters = Record<FilterKey, string>;

const FILTER_NAMES: Record<FilterKey, string> = {
  search: 'Search', gender: 'Gender', bloodGroup: 'Blood group', maritalStatus: 'Marital status',
  city: 'City', minAge: 'Min age', maxAge: 'Max age', sortBy: 'Sort', sortDirection: 'Order',
};

function readFilters(params: URLSearchParams): Filters {
  return Object.fromEntries(FILTER_KEYS.map(k => [k, params.get(k) ?? ''])) as Filters;
}

function toQuery(params: URLSearchParams): PersonQueryParameters {
  const f = readFilters(params);
  return {
    ...(f as unknown as PersonQueryParameters),
    minAge: f.minAge ? Number(f.minAge) : undefined,
    maxAge: f.maxAge ? Number(f.maxAge) : undefined,
    page: Number(params.get('page') ?? 1),
    pageSize: 10,
  };
}

function writeParams(filters: Filters) {
  const next = new URLSearchParams();
  for (const [k, v] of Object.entries(filters)) if (v.trim()) next.set(k, v.trim());
  return next; // page resets to 1
}

export function PersonListPage() {
  const navigate = useNavigate();
  const [params, setParams] = useSearchParams();
  const applied = readFilters(params);
  const [draft, setDraft] = useState<Filters>(applied);
  const advancedCount = FILTER_KEYS.filter(k => k !== 'search' && applied[k]).length;
  const [showFilters, setShowFilters] = useState(advancedCount > 0);

  const queryKey = params.toString();
  const { data, error, loading } = useApi(signal => personsApi.search(toQuery(params), signal), [queryKey]);

  const set = (key: FilterKey) => (e: { target: { value: string } }) => setDraft(d => ({ ...d, [key]: e.target.value }));

  function applyFilters(e: FormEvent) {
    e.preventDefault();
    setParams(writeParams(draft));
  }

  function clearFilter(key: FilterKey) {
    const next = { ...applied, [key]: '' };
    setDraft(next);
    setParams(writeParams(next));
  }

  function resetFilters() {
    const empty = readFilters(new URLSearchParams());
    setDraft(empty);
    setParams(new URLSearchParams());
  }

  function goToPage(page: number) {
    const next = new URLSearchParams(params);
    next.set('page', String(page));
    setParams(next);
  }

  const chips = FILTER_KEYS.filter(k => applied[k]);
  const chipValue = (k: FilterKey) =>
    k === 'sortDirection' ? (applied[k] === 'Desc' ? 'Descending' : 'Ascending') : k === 'search' || k === 'city' || k.endsWith('Age') ? applied[k] : label(applied[k]);

  return (
    <div className="page">
      <header className="page-header">
        <div>
          <h1>People</h1>
          <p className="subtitle">
            {data ? `${data.totalCount} ${data.totalCount === 1 ? 'record' : 'records'}` : 'Loading records…'}
          </p>
        </div>
        <Link to="/persons/new" className="btn btn-primary">
          <Icon name="plus" size={16} /> Add person
        </Link>
      </header>

      <form className="toolbar" onSubmit={applyFilters}>
        <div className="search-box">
          <Icon name="search" size={18} />
          <input type="search" placeholder="Search by name or e-mail…" value={draft.search} onChange={set('search')} aria-label="Search" />
        </div>
        <button type="button" className={`btn ${showFilters ? 'btn-pressed' : ''}`} onClick={() => setShowFilters(s => !s)} aria-expanded={showFilters}>
          <Icon name="sliders" size={16} /> Filters
          {advancedCount > 0 && <span className="pill-count">{advancedCount}</span>}
        </button>
        <button type="submit" className="btn btn-primary">Search</button>

        {showFilters && (
          <div className="filter-panel">
            <label className="field">
              <span>Gender</span>
              <select value={draft.gender} onChange={set('gender')}>
                <option value="">Any</option>
                {GENDERS.map(g => <option key={g} value={g}>{label(g)}</option>)}
              </select>
            </label>
            <label className="field">
              <span>Blood group</span>
              <select value={draft.bloodGroup} onChange={set('bloodGroup')}>
                <option value="">Any</option>
                {BLOOD_GROUPS.map(b => <option key={b} value={b}>{label(b)}</option>)}
              </select>
            </label>
            <label className="field">
              <span>Marital status</span>
              <select value={draft.maritalStatus} onChange={set('maritalStatus')}>
                <option value="">Any</option>
                {MARITAL_STATUSES.map(m => <option key={m} value={m}>{label(m)}</option>)}
              </select>
            </label>
            <label className="field">
              <span>City</span>
              <input value={draft.city} onChange={set('city')} placeholder="Any city" />
            </label>
            <div className="field">
              <span>Age range</span>
              <div className="range">
                <input type="number" min={0} max={150} placeholder="Min" value={draft.minAge} onChange={set('minAge')} aria-label="Minimum age" />
                <span className="muted">to</span>
                <input type="number" min={0} max={150} placeholder="Max" value={draft.maxAge} onChange={set('maxAge')} aria-label="Maximum age" />
              </div>
            </div>
            <label className="field">
              <span>Sort by</span>
              <select value={draft.sortBy} onChange={set('sortBy')}>
                <option value="">Last name</option>
                {SORT_FIELDS.filter(s => s !== 'LastName').map(s => <option key={s} value={s}>{label(s)}</option>)}
              </select>
            </label>
            <label className="field">
              <span>Order</span>
              <select value={draft.sortDirection} onChange={set('sortDirection')}>
                <option value="">Ascending</option>
                <option value="Desc">Descending</option>
              </select>
            </label>
            <div className="filter-actions">
              <button type="submit" className="btn btn-primary">Apply filters</button>
              <button type="button" className="btn btn-ghost" onClick={resetFilters}>Reset</button>
            </div>
          </div>
        )}
      </form>

      {chips.length > 0 && (
        <div className="chips">
          {chips.map(k => (
            <button key={k} type="button" className="chip" onClick={() => clearFilter(k)} title="Remove filter">
              <span className="muted">{FILTER_NAMES[k]}:</span> {chipValue(k)} <Icon name="x" size={14} />
            </button>
          ))}
          <button type="button" className="btn-link" onClick={resetFilters}>Clear all</button>
        </div>
      )}

      <ErrorMessage error={error} />

      <section className="card table-card" aria-busy={loading}>
        {!data && loading && <SkeletonRows rows={4} />}

        {data && data.items.length === 0 && (
          <div className="empty-state">
            <span className="empty-icon"><Icon name="users" size={28} /></span>
            <h3>No people found</h3>
            <p className="muted">{chips.length ? 'Try removing a filter or searching for something else.' : 'Add the first person to get started.'}</p>
            {chips.length ? (
              <button type="button" className="btn" onClick={resetFilters}>Clear filters</button>
            ) : (
              <Link to="/persons/new" className="btn btn-primary"><Icon name="plus" size={16} /> Add person</Link>
            )}
          </div>
        )}

        {data && data.items.length > 0 && (
          <div className="table-scroll">
            <table className="people-table">
              <thead>
                <tr>
                  <th>Name</th>
                  <th className="hide-sm">Phone</th>
                  <th className="num">Age</th>
                  <th className="hide-sm">Gender</th>
                  <th className="hide-md">Nationality</th>
                  <th className="hide-sm">City</th>
                  <th aria-label="Open" />
                </tr>
              </thead>
              <tbody className={loading ? 'stale' : undefined}>
                {data.items.map(p => (
                  // The whole row opens the person; the name stays a real link for keyboard and screen-reader users.
                  <tr key={p.id} className="clickable" onClick={() => navigate(`/persons/${p.id}`)}>
                    <td>
                      <div className="person-cell">
                        <Avatar name={p.fullName} />
                        <div>
                          <Link to={`/persons/${p.id}`} className="person-name" onClick={e => e.stopPropagation()}>{p.fullName}</Link>
                          <div className="person-sub">{p.email}</div>
                        </div>
                      </div>
                    </td>
                    <td className="nowrap hide-sm">{p.primaryPhone}</td>
                    <td className="num">{p.age}</td>
                    <td className="hide-sm"><span className="tag">{label(p.gender)}</span></td>
                    <td className="hide-md">{p.nationality}</td>
                    <td className="hide-sm">
                      {p.city ? <span className="with-icon"><Icon name="pin" size={14} />{p.city}</span> : <span className="muted">—</span>}
                    </td>
                    <td className="row-arrow"><Icon name="chevron" size={16} /></td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
        {data && data.totalCount > 0 && <Pagination result={data} onPageChange={goToPage} />}
      </section>
    </div>
  );
}
