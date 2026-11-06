import { personsApi } from '../api/persons';
import { BarList } from '../components/BarList';
import { ErrorMessage } from '../components/ErrorMessage';
import { Icon, type IconName } from '../components/Icon';
import { Skeleton } from '../components/Skeleton';
import { useApi } from '../hooks/useApi';

// Buckets produced by PersonService.ToAgeGroup; the API sorts them by count, the chart shows them by age.
const AGE_GROUPS = ['Under 18', '18-29', '30-44', '45-59', '60+'];

/** The key with the highest count, e.g. the most common nationality. */
function top(data: Record<string, number>) {
  const entries = Object.entries(data).sort((a, b) => b[1] - a[1]);
  return entries[0];
}

function Tile({ icon, name, value, note }: { icon: IconName; name: string; value: string | number; note?: string }) {
  return (
    <div className="card tile">
      <span className="tile-icon"><Icon name={icon} size={18} /></span>
      <span className="tile-name">{name}</span>
      <span className="tile-value">{value}</span>
      {note && <span className="tile-note">{note}</span>}
    </div>
  );
}

export function StatisticsPage() {
  const { data, error, loading } = useApi(signal => personsApi.statistics(signal), []);
  const topNationality = data && top(data.byNationality);
  const topAgeGroup = data && top(data.byAgeGroup);

  return (
    <div className="page">
      <header className="page-header">
        <div>
          <h1>Statistics</h1>
          <p className="subtitle">An overview of everyone stored in the system.</p>
        </div>
      </header>

      <ErrorMessage error={error} />

      {loading && !data && (
        <div className="tiles">
          {[0, 1, 2, 3].map(i => <div key={i} className="card tile"><Skeleton width="50%" /><Skeleton width="40%" height={30} /></div>)}
        </div>
      )}

      {data && (
        <>
          <div className="tiles">
            <Tile icon="users" name="People stored" value={data.totalPersons} />
            <Tile icon="cake" name="Average age" value={data.averageAge.toFixed(1)} note="years" />
            <Tile icon="globe" name="Top nationality" value={topNationality?.[0] ?? '—'} note={topNationality && `${topNationality[1]} of ${data.totalPersons}`} />
            <Tile icon="calendar" name="Largest age group" value={topAgeGroup?.[0] ?? '—'} note={topAgeGroup && `${topAgeGroup[1]} of ${data.totalPersons}`} />
          </div>
          <div className="chart-grid">
            <BarList title="By age group" data={data.byAgeGroup} order={AGE_GROUPS} />
            <BarList title="By gender" data={data.byGender} formatKeys />
            <BarList title="By blood group" data={data.byBloodGroup} formatKeys />
            <BarList title="By nationality" data={data.byNationality} />
          </div>
        </>
      )}
    </div>
  );
}
