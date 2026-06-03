using Microsoft.Net.Http.Headers;

namespace ESAM.GrowTracking.API.Middleware
{
    public sealed class SecurityHeadersMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IHostEnvironment _env;
        private const string DevelopmentCsp = "default-src 'none'; connect-src 'self' http: https:; img-src 'self' data: https:; style-src 'self' 'unsafe-inline' https:; " +
            "font-src 'self' data: https:; script-src 'self' 'unsafe-inline' https:; worker-src 'self' blob:; form-action 'self'; object-src 'none'; frame-ancestors 'none'; " +
            "base-uri 'none';";
        private const string ProductionCsp = "default-src 'none'; connect-src 'self' https:; img-src 'self' data: https:; style-src 'self' https:; font-src 'self' data:; " +
            "script-src 'self' https:; worker-src 'self' blob:; form-action 'self'; object-src 'none'; frame-ancestors 'none'; base-uri 'none'; upgrade-insecure-requests;";

        public SecurityHeadersMiddleware(RequestDelegate next, IHostEnvironment env)
        {
            ArgumentNullException.ThrowIfNull(next);
            ArgumentNullException.ThrowIfNull(env);
            _next = next;
            _env = env;
        }

        public async Task Invoke(HttpContext context)
        {
            ArgumentNullException.ThrowIfNull(context);
            var isHttps = context.Request.IsHttps;
            context.Response.OnStarting(() =>
            {
                ApplyHeaders(context.Response.Headers, isHttps);
                return Task.CompletedTask;
            });
            await _next(context);
        }

        private void ApplyHeaders(IHeaderDictionary headers, bool isHttps)
        {
            headers.Remove("X-Powered-By");
            if (!headers.ContainsKey(HeaderNames.XContentTypeOptions))
                headers[HeaderNames.XContentTypeOptions] = "nosniff";
            if (!headers.ContainsKey(HeaderNames.XFrameOptions))
                headers[HeaderNames.XFrameOptions] = "DENY";
            if (!headers.ContainsKey("Referrer-Policy"))
                headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
            if (!headers.ContainsKey("Permissions-Policy"))
                headers["Permissions-Policy"] = "geolocation=(), microphone=(), camera=(), interest-cohort=()";
            if (!headers.ContainsKey("X-Permitted-Cross-Domain-Policies"))
                headers["X-Permitted-Cross-Domain-Policies"] = "none";
            if (!headers.ContainsKey("Cross-Origin-Opener-Policy"))
                headers["Cross-Origin-Opener-Policy"] = "same-origin";
            if (!headers.ContainsKey("Cross-Origin-Resource-Policy"))
                headers["Cross-Origin-Resource-Policy"] = "cross-origin";
            if (!headers.ContainsKey("Content-Security-Policy"))
                headers.ContentSecurityPolicy = _env.IsDevelopment() ? DevelopmentCsp : ProductionCsp;
            if (_env.IsProduction() && isHttps && !headers.ContainsKey(HeaderNames.StrictTransportSecurity))
                headers[HeaderNames.StrictTransportSecurity] = "max-age=15552000; includeSubDomains; preload";
        }
    }

    //public sealed class SecurityHeadersMiddleware
    //{
    //    private readonly RequestDelegate _next;
    //    private readonly IHostEnvironment _env;
    //    private const string DevelopmentCsp = "default-src 'none'; connect-src 'self' http: https:; img-src 'self' data: https:; style-src 'self' 'unsafe-inline' https:; " +
    //        "font-src 'self' data: https:; script-src 'self' 'unsafe-inline' https:; worker-src 'self' blob:; form-action 'self'; object-src 'none'; frame-ancestors 'none'; " +
    //        "base-uri 'none';";
    //    private const string ProductionCsp = "default-src 'none'; connect-src 'self' https:; img-src 'self' data: https:; style-src 'self' https:; font-src 'self' data:; " +
    //        "script-src 'self' https:; worker-src 'self' blob:; form-action 'self'; object-src 'none'; frame-ancestors 'none'; base-uri 'none'; upgrade-insecure-requests;";

    //    public SecurityHeadersMiddleware(RequestDelegate next, IHostEnvironment env)
    //    {
    //        ArgumentNullException.ThrowIfNull(next);
    //        ArgumentNullException.ThrowIfNull(env);
    //        _next = next;
    //        _env = env;
    //    }

    //    public async Task Invoke(HttpContext context)
    //    {
    //        ArgumentNullException.ThrowIfNull(context);
    //        var isHttps = context.Request.IsHttps;
    //        context.Response.OnStarting(() =>
    //        {
    //            ApplyHeaders(context.Response.Headers, isHttps);
    //            return Task.CompletedTask;
    //        });
    //        await _next(context);
    //    }

    //    private void ApplyHeaders(IHeaderDictionary headers, bool isHttps)
    //    {
    //        headers.Remove("X-Powered-By");
    //        if (!headers.ContainsKey(HeaderNames.XContentTypeOptions))
    //            headers[HeaderNames.XContentTypeOptions] = "nosniff";
    //        if (!headers.ContainsKey(HeaderNames.XFrameOptions))
    //            headers[HeaderNames.XFrameOptions] = "DENY";
    //        // Ajuste: deshabilitar el filtro XSS legacy (deprecado en Chrome 78+, explotable en IE/Edge Legacy).
    //        // OWASP recomienda X-XSS-Protection: 0 para evitar ataques basados en el auditor XSS del navegador.
    //        if (!headers.ContainsKey("X-XSS-Protection"))
    //            headers["X-XSS-Protection"] = "0";
    //        if (!headers.ContainsKey("Referrer-Policy"))
    //            headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
    //        if (!headers.ContainsKey("Permissions-Policy"))
    //            headers["Permissions-Policy"] = "geolocation=(), microphone=(), camera=(), interest-cohort=()";
    //        if (!headers.ContainsKey("X-Permitted-Cross-Domain-Policies"))
    //            headers["X-Permitted-Cross-Domain-Policies"] = "none";
    //        if (!headers.ContainsKey("Cross-Origin-Opener-Policy"))
    //            headers["Cross-Origin-Opener-Policy"] = "same-origin";
    //        if (!headers.ContainsKey("Cross-Origin-Resource-Policy"))
    //            headers["Cross-Origin-Resource-Policy"] = "cross-origin";
    //        if (!headers.ContainsKey("Content-Security-Policy"))
    //            headers.ContentSecurityPolicy = _env.IsDevelopment() ? DevelopmentCsp : ProductionCsp;
    //        // Ajuste: HSTS eliminado de aquí. Bug original: los callbacks OnStarting se ejecutan en orden
    //        // LIFO (el último registrado se ejecuta primero). UseHsts() es middleware interno, registra su
    //        // callback DESPUÉS que este middleware y por tanto se ejecuta PRIMERO, seteando el valor por
    //        // defecto de ASP.NET Core (30 días, sin preload). Cuando este callback ejecutaba luego,
    //        // encontraba el header ya presente y lo omitía — el valor personalizado nunca se aplicaba.
    //        // HSTS se configura ahora exclusivamente mediante AddAPIHsts() + UseHsts() en Program.cs.
    //    }
    //}
}