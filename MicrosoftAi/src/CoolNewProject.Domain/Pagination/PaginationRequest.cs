namespace CoolNewProject.Domain.Pagination;

public sealed record PaginationRequest(int PageSize = 10, int PageIndex = 0);
