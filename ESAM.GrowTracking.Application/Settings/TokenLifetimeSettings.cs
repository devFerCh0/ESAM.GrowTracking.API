namespace ESAM.GrowTracking.Application.Settings
{
    public sealed class TokenLifetimeSettings
    {
        public int TemporaryAccessTokenLifetimeMinutes { get; init; } = 5;

        public int SessionAccessTokenLifetimeMinutes { get; init; } = 15;

        public int RefreshTokenLifetimeDays { get; init; } = 7;

        public int SessionAbsoluteLifetimeDays { get; init; } = 30;

        public int SessionIdleWindowDays { get; init; } = 3;

        public void Validate(bool isProduction)
        {
            if (TemporaryAccessTokenLifetimeMinutes <= 0)
                throw new InvalidOperationException($"{nameof(TemporaryAccessTokenLifetimeMinutes)} debe ser mayor a cero.");
            if (TemporaryAccessTokenLifetimeMinutes > 30)
                throw new InvalidOperationException($"{nameof(TemporaryAccessTokenLifetimeMinutes)} no debe superar los 30 minutos. " +
                    "Los tokens de acceso temporal deben tener una vida útil corta para minimizar la ventana de exposición ante uso indebido.");
            if (SessionAccessTokenLifetimeMinutes <= 0)
                throw new InvalidOperationException($"{nameof(SessionAccessTokenLifetimeMinutes)} debe ser mayor a cero.");
            if (SessionAccessTokenLifetimeMinutes > 1440)
                throw new InvalidOperationException($"{nameof(SessionAccessTokenLifetimeMinutes)} no debe superar los 1440 minutos (24 horas). " +
                    "Los access tokens de larga duración incrementan el riesgo ante compromisos de token.");
            if (RefreshTokenLifetimeDays <= 0)
                throw new InvalidOperationException($"{nameof(RefreshTokenLifetimeDays)} debe ser mayor a cero.");
            if (RefreshTokenLifetimeDays > 90)
                throw new InvalidOperationException($"{nameof(RefreshTokenLifetimeDays)} no debe superar los 90 días " +
                    "para acotar la ventana de exposición del refresh token ante robo o filtración.");
            if (SessionAbsoluteLifetimeDays <= 0)
                throw new InvalidOperationException($"{nameof(SessionAbsoluteLifetimeDays)} debe ser mayor a cero.");
            if (SessionAbsoluteLifetimeDays < RefreshTokenLifetimeDays)
                throw new InvalidOperationException($"{nameof(SessionAbsoluteLifetimeDays)} debe ser mayor o igual a {nameof(RefreshTokenLifetimeDays)}. " +
                    "La sesión no puede expirar antes que el refresh token que la sostiene, lo que generaría un estado inconsistente.");
            if (SessionIdleWindowDays <= 0)
                throw new InvalidOperationException($"{nameof(SessionIdleWindowDays)} debe ser mayor a cero.");
            if (SessionIdleWindowDays >= SessionAbsoluteLifetimeDays)
                throw new InvalidOperationException($"{nameof(SessionIdleWindowDays)} debe ser estrictamente menor a {nameof(SessionAbsoluteLifetimeDays)}. " +
                    "La ventana de inactividad no puede igualar ni superar el tiempo de vida absoluto de la sesión.");
            if (isProduction)
            {
                if (TemporaryAccessTokenLifetimeMinutes > 10)
                    throw new InvalidOperationException($"{nameof(TemporaryAccessTokenLifetimeMinutes)} no debe superar los 10 minutos en producción " +
                        "para garantizar que los tokens de uso único expiren en un intervalo seguro.");
                if (SessionAccessTokenLifetimeMinutes > 60)
                    throw new InvalidOperationException($"{nameof(SessionAccessTokenLifetimeMinutes)} no debe superar los 60 minutos en producción " +
                        "para reducir la ventana de exposición del access token ante compromisos de sesión.");
            }
        }
    }
}