using Microsoft.EntityFrameworkCore;
using Quiniela.Model;

namespace Quiniela.BusinessLogic.Crud;

public sealed class ForecastBusinessLogic : EntityBusinessLogic<Forecast>
{
	public ForecastBusinessLogic(QuinielaDbContext context)
		: base(context, nameof(Forecast.ForecastId))
	{
	}

	protected override IQueryable<Forecast> IncludeNavigationProperties(IQueryable<Forecast> query)
	{
		return query
			.Include(forecast => forecast.Match)
			.Include(forecast => forecast.User);
	}
}
