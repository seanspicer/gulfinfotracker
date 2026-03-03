export function SkeletonCard() {
  return (
    <div className="bg-white rounded-lg shadow-sm p-4 animate-pulse">
      <div className="h-4 bg-gray-200 rounded w-3/4 mb-2" />
      <div className="h-3 bg-gray-200 rounded w-1/2 mb-3" />
      <div className="h-3 bg-gray-200 rounded w-full mb-1" />
      <div className="h-3 bg-gray-200 rounded w-5/6" />
    </div>
  )
}
