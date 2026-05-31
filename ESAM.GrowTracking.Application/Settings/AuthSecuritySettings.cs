namespace ESAM.GrowTracking.Application.Settings
{
    public sealed class AuthSecuritySettings
    {
        public int MaxFailedAttempts { get; set; } = 5;

        public TimeSpan LockoutDuration { get; set; } = TimeSpan.FromMinutes(15);

        public TimeSpan FailedAttemptsResetDuration { get; set; } = TimeSpan.FromHours(1);

        public void Validate(bool isProduction)
        {
            if (MaxFailedAttempts <= 0)
                throw new InvalidOperationException($"{nameof(MaxFailedAttempts)} debe ser mayor a cero.");
            if (MaxFailedAttempts > 20)
                throw new InvalidOperationException($"{nameof(MaxFailedAttempts)} no debe superar 20 intentos. " +
                    "Valores excesivamente permisivos reducen la efectividad de la política de bloqueo ante ataques de fuerza bruta.");
            if (LockoutDuration <= TimeSpan.Zero)
                throw new InvalidOperationException($"{nameof(LockoutDuration)} debe ser un intervalo de tiempo positivo.");
            if (LockoutDuration > TimeSpan.FromHours(24))
                throw new InvalidOperationException($"{nameof(LockoutDuration)} no debe superar las 24 horas. " +
                    "Un bloqueo excesivamente prolongado puede derivar en denegación de servicio para usuarios legítimos.");
            if (FailedAttemptsResetDuration <= TimeSpan.Zero)
                throw new InvalidOperationException($"{nameof(FailedAttemptsResetDuration)} debe ser un intervalo de tiempo positivo.");
            if (FailedAttemptsResetDuration < LockoutDuration)
                throw new InvalidOperationException($"{nameof(FailedAttemptsResetDuration)} debe ser mayor o igual a {nameof(LockoutDuration)}. " +
                    "La ventana de reinicio de intentos fallidos no puede expirar antes de que concluya el período de bloqueo activo.");
            if (isProduction)
            {
                if (MaxFailedAttempts > 10)
                    throw new InvalidOperationException($"{nameof(MaxFailedAttempts)} no debe superar 10 intentos en producción " +
                        "para limitar eficazmente los ataques de fuerza bruta.");
                if (LockoutDuration < TimeSpan.FromMinutes(5))
                    throw new InvalidOperationException($"{nameof(LockoutDuration)} debe ser al menos 5 minutos en producción " +
                        "para disuadir ataques de fuerza bruta sostenidos.");
            }
        }
    }
}