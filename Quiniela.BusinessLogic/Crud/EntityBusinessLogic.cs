using Microsoft.EntityFrameworkCore;
using Quiniela.Model;

namespace Quiniela.BusinessLogic.Crud;

public abstract class EntityBusinessLogic<TEntity>
	where TEntity : class
{
	private readonly string _primaryKeyProperty;

	protected EntityBusinessLogic(QuinielaDbContext context, string primaryKeyProperty)
	{
		Context = context;
		Entities = context.Set<TEntity>();
		_primaryKeyProperty = primaryKeyProperty;
	}

	protected QuinielaDbContext Context { get; }

	protected DbSet<TEntity> Entities { get; }

	protected abstract IQueryable<TEntity> IncludeNavigationProperties(IQueryable<TEntity> query);

	public async Task<TEntity> InsertAsync(
		TEntity entity,
		CancellationToken cancellationToken = default)
	{
		Context.Entry(entity).State = EntityState.Added;
		await Context.SaveChangesAsync(cancellationToken);
		return entity;
	}

	public async Task<TEntity> UpdateAsync(
		TEntity entity,
		CancellationToken cancellationToken = default)
	{
		Context.Entry(entity).State = EntityState.Modified;
		await Context.SaveChangesAsync(cancellationToken);
		return entity;
	}

	public async Task<bool> DeleteAsync(
		int id,
		CancellationToken cancellationToken = default)
	{
		var entity = await Entities.FindAsync(new object?[] { id }, cancellationToken);
		if (entity is null)
		{
			return false;
		}

		Context.Entry(entity).State = EntityState.Deleted;
		await Context.SaveChangesAsync(cancellationToken);
		return true;
	}

	public Task<TEntity?> GetByIdAsync(
		int id,
		CancellationToken cancellationToken = default)
	{
		var query = IncludeNavigationProperties(Entities.AsNoTracking());
		return query.SingleOrDefaultAsync(
			entity => EF.Property<int>(entity, _primaryKeyProperty) == id,
			cancellationToken);
	}

	/// <summary>
	/// Returns one page of entities ordered by their primary key.
	/// </summary>
	/// <param name="pageNumber">The one-based page number (defaults to 1).</param>
	/// <param name="pageSize">The number of entities per page (defaults to 10).</param>
	/// <param name="cancellationToken">Optional cancellation token.</param>
	public Task<List<TEntity>> GetAllAsync(
		int pageNumber = 1,
		int pageSize = 10,
		CancellationToken cancellationToken = default)
	{
		if (pageNumber < 1)
		{
			throw new ArgumentOutOfRangeException(nameof(pageNumber), "Page number must be greater than zero.");
		}

		if (pageSize < 1)
		{
			throw new ArgumentOutOfRangeException(nameof(pageSize), "Page size must be greater than zero.");
		}

		var offset = checked((pageNumber - 1) * pageSize);

		return IncludeNavigationProperties(Entities.AsNoTracking())
			.OrderBy(entity => EF.Property<int>(entity, _primaryKeyProperty))
			.Skip(offset)
			.Take(pageSize)
			.ToListAsync(cancellationToken);
	}
}
