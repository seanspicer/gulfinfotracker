namespace GulfInfoTracker.Api.Models;

public record PagedResult<T>(
    IReadOnlyList<T> Data,
    int Total,
    int Page,
    int PageSize
);
