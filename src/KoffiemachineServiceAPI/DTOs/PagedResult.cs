namespace KoffiemachineServiceAPI.DTOs;

public record PagedResult<T>
{
    public int TotalRecords { get; init; }
    public int PageSize { get; init; }
    public int CurrentPage { get; init; }
    public int TotalPages => (int)Math.Ceiling((double)TotalRecords / PageSize);
    public IReadOnlyList<T> Data { get; init; } = Array.Empty<T>();
}
