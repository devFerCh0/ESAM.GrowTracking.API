namespace ESAM.GrowTracking.Application.Settings
{
    public sealed class CleanupSettings
    {
        public TimeSpan InitialDelay { get; init; } = TimeSpan.FromMinutes(1);

        public TimeSpan Interval { get; init; } = TimeSpan.FromHours(1);

        public int BatchSize { get; init; } = 1000;

        public void Validate()
        {
            if (InitialDelay < TimeSpan.Zero)
                throw new InvalidOperationException($"{nameof(InitialDelay)} no puede ser un intervalo de tiempo negativo.");
            if (Interval <= TimeSpan.Zero)
                throw new InvalidOperationException($"{nameof(Interval)} debe ser un intervalo de tiempo positivo.");
            if (Interval < TimeSpan.FromMinutes(1))
                throw new InvalidOperationException($"{nameof(Interval)} no debe ser inferior a 1 minuto " +
                    "para evitar una carga excesiva y sostenida sobre la base de datos.");
            if (Interval > TimeSpan.FromHours(24))
                throw new InvalidOperationException($"{nameof(Interval)} no debe superar las 24 horas para garantizar " +
                    "que la limpieza de datos expirados se ejecute con frecuencia suficiente.");
            if (BatchSize <= 0)
                throw new InvalidOperationException($"{nameof(BatchSize)} debe ser mayor a cero.");
            if (BatchSize > 10_000)
                throw new InvalidOperationException($"{nameof(BatchSize)} no debe superar los 10 000 registros por lote " +
                    "para evitar bloqueos prolongados en la base de datos y presión excesiva sobre el plan de ejecución.");
        }
    }
}