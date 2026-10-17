namespace Quiniela.Api.Services;

/// <summary>CRUD operations exposed by an API resource service.</summary>
public interface IEntityApiService<TRequest, TResponse>
{
    Task<IReadOnlyList<TResponse>> GetAllAsync(
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken);

    Task<TResponse?> GetByIdAsync(int id, CancellationToken cancellationToken);

    Task<(int Id, TResponse Response)> CreateAsync(
        TRequest request,
        CancellationToken cancellationToken);

    Task<TResponse?> UpdateAsync(
        int id,
        TRequest request,
        CancellationToken cancellationToken);

    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken);
}
