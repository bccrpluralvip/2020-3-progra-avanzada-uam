using Microsoft.EntityFrameworkCore;
using Quiniela.Model;

namespace Quiniela.BusinessLogic.Crud;

public sealed class UserBusinessLogic : EntityBusinessLogic<User>
{
	public UserBusinessLogic(QuinielaDbContext context)
		: base(context, nameof(User.UserId))
	{
	}

	protected override IQueryable<User> IncludeNavigationProperties(IQueryable<User> query)
	{
		return query.Include(user => user.Forecasts);
	}
}
