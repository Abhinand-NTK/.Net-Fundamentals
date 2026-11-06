import { useCallback, useEffect, useState } from 'react';

interface ApiState<T> {
  data: T | undefined;
  error: unknown;
  loading: boolean;
  /** Runs the request again (e.g. after a delete). */
  reload: () => void;
}

/**
 * Loads data when the component mounts and whenever `deps` change.
 * Each run gets its own AbortController; when deps change or the component unmounts,
 * the previous request is aborted, so a slow old response can never overwrite a newer one.
 * The API receives the abort as a cancelled CancellationToken.
 */
export function useApi<T>(load: (signal: AbortSignal) => Promise<T>, deps: unknown[]): ApiState<T> {
  const [data, setData] = useState<T>();
  const [error, setError] = useState<unknown>();
  const [loading, setLoading] = useState(true);
  const [reloadCount, setReloadCount] = useState(0);

  useEffect(() => {
    const controller = new AbortController();
    setLoading(true);
    setError(undefined);

    load(controller.signal)
      .then(result => setData(result))
      .catch(err => {
        if (!controller.signal.aborted) setError(err);
      })
      .finally(() => {
        if (!controller.signal.aborted) setLoading(false);
      });

    return () => controller.abort();
    // `load` is usually an inline arrow function (new every render), so it is deliberately not a
    // dependency; the caller lists what the request depends on in `deps` instead.
  }, [...deps, reloadCount]);

  const reload = useCallback(() => setReloadCount(c => c + 1), []);
  return { data, error, loading, reload };
}
