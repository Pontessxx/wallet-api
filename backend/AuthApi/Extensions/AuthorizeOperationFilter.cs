using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace AuthApi.Extensions;

public sealed class AuthorizeOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var method = context.MethodInfo;

        // Ignora se possuir AllowAnonymous
        var allowAnonymous =
            method.GetCustomAttributes(true).OfType<AllowAnonymousAttribute>().Any() ||
            method.DeclaringType?.GetCustomAttributes(true)
                .OfType<AllowAnonymousAttribute>()
                .Any() == true;

        if (allowAnonymous)
            return;

        // Procura Authorize
        var hasAuthorize =
            method.GetCustomAttributes(true).OfType<AuthorizeAttribute>().Any() ||
            method.DeclaringType?.GetCustomAttributes(true)
                .OfType<AuthorizeAttribute>()
                .Any() == true;

        if (!hasAuthorize)
            return;

        operation.Security ??= [];

        operation.Security.Add(
            new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecuritySchemeReference("Bearer", context.Document),
                    []
                }
            });
    }
}