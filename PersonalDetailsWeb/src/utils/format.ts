const BLOOD_GROUP_LABELS: Record<string, string> = {
  Unknown: 'Unknown',
  APositive: 'A+',
  ANegative: 'A−',
  BPositive: 'B+',
  BNegative: 'B−',
  ABPositive: 'AB+',
  ABNegative: 'AB−',
  OPositive: 'O+',
  ONegative: 'O−',
};

/** Turns an enum name into display text: "PreferNotToSay" → "Prefer not to say", "APositive" → "A+". */
export function label(value: string | undefined | null): string {
  if (!value) return '';
  if (value in BLOOD_GROUP_LABELS) return BLOOD_GROUP_LABELS[value];
  const words = value.replace(/([a-z])([A-Z])/g, '$1 $2').split(' ');
  return words.map((w, i) => (i === 0 ? w : w.toLowerCase())).join(' ');
}

/** Formats an API DateOnly ("2024-03-15") without time-zone shifts. */
export function formatDate(value: string | undefined | null): string {
  if (!value) return '';
  const [y, m, d] = value.split('-').map(Number);
  return new Date(y, m - 1, d).toLocaleDateString(undefined, { year: 'numeric', month: 'short', day: 'numeric' });
}

/** Formats an API UTC timestamp ("2026-11-05T10:18:21Z") in the viewer's local time. */
export function formatDateTime(value: string | undefined | null): string {
  if (!value) return '';
  return new Date(value).toLocaleString(undefined, { dateStyle: 'medium', timeStyle: 'short' });
}
