// Coloured initials. The colour is derived from the name, so a person keeps the same colour everywhere.
const HUES = ['blue', 'orange', 'aqua', 'violet', 'magenta', 'green', 'yellow', 'red'] as const;

function hueFor(name: string) {
  // FNV-1a hash: spreads similar names across the hues better than a simple sum.
  let hash = 2166136261;
  for (const ch of name) hash = Math.imul(hash ^ ch.charCodeAt(0), 16777619) >>> 0;
  return HUES[hash % HUES.length];
}

function initials(name: string) {
  const parts = name.trim().split(/\s+/).filter(p => !/^[A-Z]\.$/.test(p)); // skip middle initials like "J."
  const first = parts[0]?.[0] ?? '?';
  const last = parts.length > 1 ? parts[parts.length - 1][0] : '';
  return (first + last).toUpperCase();
}

export function Avatar({ name, size = 'md' }: { name: string; size?: 'sm' | 'md' | 'lg' }) {
  return (
    <span className={`avatar avatar-${size} hue-${hueFor(name)}`} aria-hidden="true">
      {initials(name)}
    </span>
  );
}
