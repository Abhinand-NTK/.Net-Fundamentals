import { ApiError } from '../api/client';
import { Icon } from './Icon';

/** Shows an error from the API (ProblemDetails) or a network failure in a consistent way. */
export function ErrorMessage({ error }: { error: unknown }) {
  if (!error) return null;

  let title = 'Something went wrong';
  let detail: string | undefined;
  let traceId: string | undefined;

  if (error instanceof ApiError) {
    title = error.problem.title ?? title;
    detail = error.problem.detail;
    traceId = error.problem.traceId;
    if (error.status >= 500 && !detail) {
      detail = 'The API could not handle the request. Check that it is running on http://localhost:5056.';
    }
  } else if (error instanceof TypeError) {
    // fetch throws TypeError when the server can't be reached at all.
    detail = 'Could not reach the API. Start it with: dotnet run --project PersonalDetailsApi --launch-profile http';
  } else if (error instanceof Error) {
    detail = error.message;
  }

  return (
    <div className="alert" role="alert">
      <Icon name="alert" size={18} />
      <div>
        <strong>{title}</strong>
        {detail && <p>{detail}</p>}
        {traceId && <p className="muted small">Trace id: {traceId}</p>}
      </div>
    </div>
  );
}
