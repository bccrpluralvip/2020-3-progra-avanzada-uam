using Microsoft.Extensions.DependencyInjection;
using Quiniela.BusinessLogic.Crud;

namespace Quiniela.Api.Services;

public static class EntityApiServiceRegistration
{
    public static IServiceCollection AddEntityApiService<TEntity, TRequest, TResponse, TBusinessLogic>(
        this IServiceCollection services,
        Func<TEntity, int> getId,
        Func<TEntity, TResponse> toResponse,
        Func<TRequest, TEntity> toEntity,
        Action<TEntity, TRequest> apply)
        where TEntity : class
        where TRequest : notnull
        where TBusinessLogic : EntityBusinessLogic<TEntity>
    {
        services.AddScoped<TBusinessLogic>();
        services.AddScoped<IEntityApiService<TRequest, TResponse>>(provider =>
            new EntityApiService<TEntity, TRequest, TResponse, TBusinessLogic>(
                provider.GetRequiredService<TBusinessLogic>(),
                getId,
                toResponse,
                toEntity,
                apply));

        return services;
    }
}

internal sealed class EntityApiService<TEntity, TRequest, TResponse, TBusinessLogic>(
    TBusinessLogic businessLogic,
    Func<TEntity, int> getId,
    Func<TEntity, TResponse> toResponse,
    Func<TRequest, TEntity> toEntity,
    Action<TEntity, TRequest> apply)
    : IEntityApiService<TRequest, TResponse>
    where TEntity : class
    where TRequest : notnull
    where TBusinessLogic : EntityBusinessLogic<TEntity>
{
    public async Task<IReadOnlyList<TResponse>> GetAllAsync(
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var entities = await businessLogic.GetAllAsync(pageNumber, pageSize, cancellationToken);
        return entities.Select(toResponse).ToArray();
    }

    public async Task<TResponse?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        var entity = await businessLogic.GetByIdAsync(id, cancellationToken);
        return entity is null ? default : toResponse(entity);
    }

    public async Task<(int Id, TResponse Response)> CreateAsync(
        TRequest request,
        CancellationToken cancellationToken)
    {
        var inserted = await businessLogic.InsertAsync(toEntity(request), cancellationToken);
        var id = getId(inserted);
        var created = await businessLogic.GetByIdAsync(id, cancellationToken)
            ?? throw new InvalidOperationException("The created resource could not be loaded.");

        return (id, toResponse(created));
    }

    public async Task<TResponse?> UpdateAsync(
        int id,
        TRequest request,
        CancellationToken cancellationToken)
    {
        var entity = await businessLogic.GetByIdAsync(id, cancellationToken);
        if (entity is null)
        {
            return default;
        }

        apply(entity, request);
        var updated = await businessLogic.UpdateAsync(entity, cancellationToken);
        var refreshed = await businessLogic.GetByIdAsync(getId(updated), cancellationToken)
            ?? throw new InvalidOperationException("The updated resource could not be loaded.");

        return toResponse(refreshed);
    }

    public Task<bool> DeleteAsync(int id, CancellationToken cancellationToken) =>
        businessLogic.DeleteAsync(id, cancellationToken);
}
