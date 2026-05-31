using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace ESAM.GrowTracking.API.Filters
{
    internal sealed class XsrfOperationFilter : IOperationFilter
    {
        private static readonly HashSet<string> StateChangingMethods = new(StringComparer.OrdinalIgnoreCase) { HttpMethods.Post, HttpMethods.Put, HttpMethods.Delete, 
            HttpMethods.Patch };

        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            ArgumentNullException.ThrowIfNull(operation);
            ArgumentNullException.ThrowIfNull(context);
            var httpMethod = context.ApiDescription.HttpMethod;
            if (httpMethod is null || !StateChangingMethods.Contains(httpMethod))
                return;
            operation.Security ??= [];
            operation.Security.Add(new OpenApiSecurityRequirement
            {
                { new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "XSRF" } }, Array.Empty<string>() }
            });
        }
    }
}