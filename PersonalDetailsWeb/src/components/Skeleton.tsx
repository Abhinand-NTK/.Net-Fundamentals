/** Grey placeholder blocks shown while data loads, so the layout doesn't jump. */
export function Skeleton({ width = '100%', height = 14, radius = 6 }: { width?: number | string; height?: number; radius?: number }) {
  return <span className="skeleton" style={{ width, height, borderRadius: radius }} />;
}

export function SkeletonRows({ rows = 5 }: { rows?: number }) {
  return (
    <div className="skeleton-rows" aria-busy="true" aria-label="Loading">
      {Array.from({ length: rows }, (_, i) => (
        <div key={i} className="skeleton-row">
          <Skeleton width={36} height={36} radius={999} />
          <div className="skeleton-text">
            <Skeleton width="40%" />
            <Skeleton width="25%" height={11} />
          </div>
        </div>
      ))}
    </div>
  );
}
