import { NavLink, Route, Routes } from 'react-router';
import { Icon } from './components/Icon';
import { PersonDetailPage } from './pages/PersonDetailPage';
import { PersonFormPage } from './pages/PersonFormPage';
import { PersonListPage } from './pages/PersonListPage';
import { StatisticsPage } from './pages/StatisticsPage';

// Swagger UI is served at the API's root URL in Development.
const API_DOCS_URL = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5056';

export function App() {
  return (
    <div className="shell">
      <aside className="sidebar">
        <div className="brand">
          <span className="brand-mark"><Icon name="idcard" size={18} /></span>
          <span className="brand-text">
            <strong>Personal Details</strong>
            <small>Records manager</small>
          </span>
        </div>
        <nav className="nav">
          <NavLink to="/" end className={({ isActive }) => (isActive ? 'active' : undefined)}>
            <Icon name="users" /> <span>People</span>
          </NavLink>
          <NavLink to="/persons/new">
            <Icon name="plus" /> <span>Add person</span>
          </NavLink>
          <NavLink to="/statistics">
            <Icon name="chart" /> <span>Statistics</span>
          </NavLink>
        </nav>
        <a className="nav-footer" href={API_DOCS_URL} target="_blank" rel="noreferrer">
          <Icon name="external" size={16} /> <span>API docs</span>
        </a>
      </aside>

      <main className="content">
        {/* Client-side routes: the browser URL changes, but no page reload happens. */}
        <Routes>
          <Route path="/" element={<PersonListPage />} />
          <Route path="/persons/new" element={<PersonFormPage />} />
          <Route path="/persons/:id" element={<PersonDetailPage />} />
          <Route path="/persons/:id/edit" element={<PersonFormPage />} />
          <Route path="/statistics" element={<StatisticsPage />} />
          <Route path="*" element={<div className="empty-state"><h2>Page not found</h2></div>} />
        </Routes>
      </main>
    </div>
  );
}
