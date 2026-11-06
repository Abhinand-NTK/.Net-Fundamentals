import { label } from '../utils/format';

interface BarListProps {
  title: string;
  data: Record<string, number>;
  /** Fixed category order (e.g. age groups youngest → oldest) instead of sorting by count. */
  order?: string[];
  /** Turn enum keys ("APositive") into display text ("A+"). */
  formatKeys?: boolean;
}

/**
 * A horizontal bar chart in plain HTML/CSS. One series, so one colour, and every bar
 * carries its value as text so the chart doesn't rely on colour or bar length alone.
 */
export function BarList({ title, data, order, formatKeys }: BarListProps) {
  const entries = order
    ? order.filter(k => k in data).map(k => [k, data[k]] as [string, number])
    : Object.entries(data).sort((a, b) => b[1] - a[1]);
  const max = Math.max(1, ...entries.map(([, v]) => v));
  const total = entries.reduce((sum, [, v]) => sum + v, 0);

  return (
    <section className="card">
      <h3>{title}</h3>
      {entries.length === 0 ? (
        <p className="muted">No data yet.</p>
      ) : (
        <ul className="bar-list">
          {entries.map(([key, value]) => {
            const name = formatKeys ? label(key) : key;
            const share = total ? Math.round((value / total) * 100) : 0;
            return (
              <li key={key} className="bar-row" title={`${name}: ${value} (${share}%)`}>
                <span className="bar-label">{name}</span>
                <span className="bar-track">
                  <span className="bar-fill" style={{ width: `${(value / max) * 100}%` }} />
                </span>
                <span className="bar-value">{value}</span>
              </li>
            );
          })}
        </ul>
      )}
    </section>
  );
}
