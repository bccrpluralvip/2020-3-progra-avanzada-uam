using Microsoft.AspNetCore.Mvc.Rendering;

namespace Quiniela.Mvc.Models;

public sealed record MatchesJsIndexViewModel(
    IReadOnlyList<SelectListItem> Tournaments,
    IReadOnlyList<SelectListItem> Teams,
    int PageSize,
    string MatchesUrl,
    string MatchUrl);
