using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Quiniela.MinimalApi.Endpoints;

internal sealed class DtoValidationFilter : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext context,
        EndpointFilterDelegate next)
    {
        foreach (var argument in context.Arguments)
        {
            if (argument is null)
            {
                continue;
            }

            var properties = argument.GetType().GetProperties();
            if (!properties.Any(property => property
                    .GetCustomAttributes(typeof(ValidationAttribute), inherit: true)
                    .Any()))
            {
                continue;
            }

            var validationResults = new List<ValidationResult>();
            if (Validator.TryValidateObject(
                    argument,
                    new ValidationContext(argument),
                    validationResults,
                    validateAllProperties: true))
            {
                continue;
            }

            var errors = validationResults
                .SelectMany(result => result.MemberNames.DefaultIfEmpty(string.Empty)
                    .Select(memberName => (memberName, result.ErrorMessage)))
                .GroupBy(error => error.memberName)
                .ToDictionary(
                    group => group.Key,
                    group => group.Select(error => error.ErrorMessage ?? "The value is invalid.").ToArray());

            return TypedResults.ValidationProblem(errors);
        }

        return await next(context);
    }
}
