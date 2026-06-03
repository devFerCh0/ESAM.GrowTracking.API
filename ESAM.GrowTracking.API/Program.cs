using ESAM.GrowTracking.API;
using ESAM.GrowTracking.API.Middleware;
using ESAM.GrowTracking.API.Security;
using ESAM.GrowTracking.Application;
using ESAM.GrowTracking.Infrastructure;
using ESAM.GrowTracking.Persistence;
using ESAM.GrowTracking.Persistence.Abstractions.Services;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddEnvironmentVariables();
if (builder.Environment.IsDevelopment())
    builder.Configuration.AddUserSecrets(Assembly.GetExecutingAssembly(), true);
else
    StartupSecurityGuard.Validate(builder.Configuration);
builder.Services.AddAPISettings(builder.Configuration, builder.Environment);
builder.Services.AddPersistenceSettings(builder.Configuration, builder.Environment);
builder.Services.AddInfrastructureSettings(builder.Configuration, builder.Environment);
builder.Services.AddApplicationSettings(builder.Configuration, builder.Environment);
builder.Services.AddAPIDataProtection();
builder.Services.AddAPIHttpContextServices();
builder.Services.AddPersistenceServices();
builder.Services.AddInfrastructureServices();
builder.Services.AddApplicationServices();
builder.Services.AddMappingProfiles();
builder.Services.AddAPICors();
builder.Services.AddAPICookie();
builder.Services.AddAPIMiddlewareServices();
builder.Services.AddAPIAuthentication();
builder.Services.AddAPIAuthorization();
builder.Services.AddAPIForwardedHeaders();
builder.Services.AddAPIRateLimiting();
builder.Services.AddAPIResponseCompression();
builder.Services.AddAPIControllers();
builder.Services.AddAPISwagger(builder.Environment);
var app = builder.Build();
app.UseForwardedHeaders();
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseMiddleware<SecurityHeadersMiddleware>();
if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
    app.UseHttpsRedirection();
}
app.UseResponseCompression();
app.UseMiddleware<StatusCodeToApiErrorMiddleware>();
app.UseRouting();
app.UseCors();
app.UseCookiePolicy();
app.UseAuthentication();
app.UseRateLimiter();
app.UseMiddleware<XsrfValidationMiddleware>();
app.UseAuthorization();
app.MapSwaggerUiCustomJs();
app.UseConfiguredSwagger();
app.MapHealthCheckEndpoints();
app.MapControllers();
await app.Services.GetRequiredService<IDatabaseMigrationService>().ApplyMigrationsAsync();
app.Run();










//var builder = WebApplication.CreateBuilder(args);
//builder.Configuration.AddEnvironmentVariables();
//if (builder.Environment.IsDevelopment())
//    builder.Configuration.AddUserSecrets(Assembly.GetExecutingAssembly(), true);
//else
//    StartupSecurityGuard.Validate(builder.Configuration);
//builder.Services.AddAPISettings(builder.Configuration, builder.Environment);
//builder.Services.AddPersistenceSettings(builder.Configuration, builder.Environment);
//builder.Services.AddInfrastructureSettings(builder.Configuration, builder.Environment);
//builder.Services.AddApplicationSettings(builder.Configuration, builder.Environment);
//builder.Services.AddAPIDataProtection();
//builder.Services.AddAPIHttpContextServices();
//builder.Services.AddPersistenceServices();
//builder.Services.AddInfrastructureServices();
//builder.Services.AddApplicationServices();
//builder.Services.AddMappingProfiles();
//builder.Services.AddAPICors();
//builder.Services.AddAPICookie();
//// Ajuste: configuración de opciones HSTS (6 meses, includeSubDomains, preload) para que UseHsts()
//// aplique los valores correctos. Ver SecurityHeadersMiddleware para detalle del bug corregido.
//builder.Services.AddAPIHsts(builder.Environment);
//builder.Services.AddAPIMiddlewareServices();
//builder.Services.AddAPIAuthentication();
//builder.Services.AddAPIAuthorization();
//builder.Services.AddAPIForwardedHeaders();
//builder.Services.AddAPIRateLimiting();
//builder.Services.AddAPIResponseCompression();
//builder.Services.AddAPIControllers();
//builder.Services.AddAPISwagger(builder.Environment);
//var app = builder.Build();
//app.UseForwardedHeaders();
//app.UseMiddleware<CorrelationIdMiddleware>();
//app.UseMiddleware<GlobalExceptionMiddleware>();
//app.UseMiddleware<SecurityHeadersMiddleware>();
//if (!app.Environment.IsDevelopment())
//{
//    app.UseHsts();            // Usa las opciones configuradas en AddAPIHsts() — 6 meses + preload
//    app.UseHttpsRedirection();
//}
//app.UseResponseCompression();
//app.UseMiddleware<StatusCodeToApiErrorMiddleware>();
//app.UseRouting();
//app.UseCors();
//app.UseCookiePolicy();
//// Ajuste: UseRateLimiter movido ANTES de UseAuthentication().
//// El rate limiter opera sobre IP (no requiere identidad del usuario). Tenerlo después de
//// UseAuthentication() forzaba la validación JWT antes de evaluar el límite de tasa,
//// incrementando el costo computacional de las solicitudes rechazadas — especialmente crítico
//// en los endpoints de autenticación, que son objetivo natural de ataques de fuerza bruta.
//app.UseRateLimiter();
//app.UseAuthentication();
//app.UseMiddleware<XsrfValidationMiddleware>();
//app.UseAuthorization();
//app.MapSwaggerUiCustomJs();
//app.UseConfiguredSwagger();
//app.MapHealthCheckEndpoints();
//app.MapControllers();
//await app.Services.GetRequiredService<IDatabaseMigrationService>().ApplyMigrationsAsync();
//app.Run();