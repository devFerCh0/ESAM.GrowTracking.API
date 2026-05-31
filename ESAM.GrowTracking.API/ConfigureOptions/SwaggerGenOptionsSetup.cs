using ESAM.GrowTracking.API.Filters;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace ESAM.GrowTracking.API.ConfigureOptions
{
    internal sealed class SwaggerGenOptionsSetup : IConfigureOptions<SwaggerGenOptions>
    {
        public void Configure(SwaggerGenOptions options)
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "GrowTracking API",
                Version = "v1",
                Description = "API del sistema GrowTracking — ESAM.\n\n**Autenticación:** Usar el esquema `Bearer` con el JWT obtenido en `/api/auth/login`.\n\n" +
                    "**XSRF:** Las operaciones de escritura (POST, PUT, DELETE, PATCH) realizadas desde un cliente " +
                    "web con cookie de refresh activa requieren el encabezado `X-XSRF-TOKEN`. " +
                    "Leer el valor de la cookie `XSRF-TOKEN` (no HttpOnly) e incluirlo en el campo XSRF de Swagger. " +
                    "Postman y apps móviles que utilicen el encabezado `X-Refresh-Token` no requieren XSRF."
            });
            options.CustomSchemaIds(ResolveSchemaId);
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                Description = "Token JWT obtenido en el endpoint de login.\n\nFormato: `Bearer {token}` (Swagger UI añade el prefijo automáticamente)."
            });
            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                { new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } }, Array.Empty<string>() }
            });
            options.AddSecurityDefinition("XSRF", new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.ApiKey,
                In = ParameterLocation.Header,
                Name = "X-XSRF-TOKEN",
                Description = "Requerido en operaciones de escritura (POST, PUT, DELETE, PATCH) cuando el cliente " +
                    "tiene activa la cookie `rt` / `__Host-rt` de refresh token (flujo cookie-based).\n\n" +
                    "**Cómo obtenerlo:** después del login, el servidor devuelve la cookie `XSRF-TOKEN` " +
                    "(no HttpOnly, legible por JavaScript). Copiar su valor y pegarlo aquí.\n\n" +
                    "**Postman / apps móviles:** no aplica cuando se usa el encabezado `X-Refresh-Token`."
            });
            options.OperationFilter<XsrfOperationFilter>();
        }

        private static string ResolveSchemaId(Type type)
        {
            if (!type.IsGenericType)
                return (type.FullName ?? type.Name).Replace("+", ".");
            var baseFullName = type.GetGenericTypeDefinition().FullName ?? type.Name;
            var baseName = baseFullName.Split('`')[0].Replace("+", ".");
            var argNames = string.Join("_", type.GetGenericArguments().Select(ResolveSchemaId));
            return $"{baseName}__{argNames}";
        }
    }
}