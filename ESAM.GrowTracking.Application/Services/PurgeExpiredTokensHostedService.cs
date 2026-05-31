using ESAM.GrowTracking.Application.Abstractions.Services;
using ESAM.GrowTracking.Application.Settings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ESAM.GrowTracking.Application.Services
{
    public sealed class PurgeExpiredTokensHostedService : BackgroundService
    {
        private readonly ILogger<PurgeExpiredTokensHostedService> _logger;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IDateTimeService _dateTimeService;
        private readonly CleanupSettings _cleanupSettings;

        public PurgeExpiredTokensHostedService(ILogger<PurgeExpiredTokensHostedService> logger, IServiceScopeFactory scopeFactory, IDateTimeService dateTimeService,
            IOptions<CleanupSettings> cleanupSettingsOptions)
        {
            ArgumentNullException.ThrowIfNull(logger);
            ArgumentNullException.ThrowIfNull(scopeFactory);
            ArgumentNullException.ThrowIfNull(dateTimeService);
            ArgumentNullException.ThrowIfNull(cleanupSettingsOptions);
            _logger = logger;
            _scopeFactory = scopeFactory;
            _dateTimeService = dateTimeService;
            _cleanupSettings = cleanupSettingsOptions.Value ?? throw new ArgumentNullException(nameof(cleanupSettingsOptions));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("PurgeExpiredTokensHostedService iniciando. InitialDelay={InitialDelay}, Interval={Interval}, BatchSize={BatchSize}", 
                _cleanupSettings.InitialDelay, _cleanupSettings.Interval, _cleanupSettings.BatchSize);
            if (!await ApplyInitialDelayAsync(stoppingToken))
                return;
            while (!stoppingToken.IsCancellationRequested)
            {
                var cycleStartedAt = _dateTimeService.UtcNow;
                await RunPurgeCycleAsync(cycleStartedAt, stoppingToken);
                if (!await WaitForNextCycleAsync(cycleStartedAt, stoppingToken))
                    break;
            }
            _logger.LogInformation("PurgeExpiredTokensHostedService detenido.");
        }

        private async Task<bool> ApplyInitialDelayAsync(CancellationToken stoppingToken)
        {
            if (_cleanupSettings.InitialDelay <= TimeSpan.Zero)
                return true;
            try
            {
                await Task.Delay(_cleanupSettings.InitialDelay, stoppingToken);
                return true;
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("PurgeExpiredTokensHostedService: cancelado durante el retraso inicial.");
                return false;
            }
        }

        private async Task RunPurgeCycleAsync(DateTime cycleStartedAt, CancellationToken stoppingToken)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var purgeService = scope.ServiceProvider.GetRequiredService<IPurgeExpiredTokensService>();
                _logger.LogInformation("PurgeExpiredTokensHostedService: ciclo de purga iniciado. StartedAt={StartedAt}", cycleStartedAt);
                var result = await purgeService.PurgeAsync(_cleanupSettings.BatchSize, cycleStartedAt, stoppingToken);
                _logger.LogInformation("PurgeExpiredTokensHostedService: ciclo de purga finalizado. StartedAt={StartedAt}, Total={Total}, " +
                    "Temporary={Temporary}, Permanent={Permanent}, BlacklistedRefresh={BlacklistedRefresh}, SessionRefresh={SessionRefresh}", cycleStartedAt, result.TotalDeleted, 
                    result.BlacklistedAccessTokensTemporaryDeleted, result.BlacklistedAccessTokensPermanentDeleted, result.BlacklistedRefreshTokensDeleted,
                    result.UserSessionRefreshTokensDeleted);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("PurgeExpiredTokensHostedService: ciclo de purga cancelado. StartedAt={StartedAt}", cycleStartedAt);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PurgeExpiredTokensHostedService: error inesperado durante el ciclo de purga. StartedAt={StartedAt}", cycleStartedAt);
            }
        }

        private async Task<bool> WaitForNextCycleAsync(DateTime cycleStartedAt, CancellationToken stoppingToken)
        {
            try
            {
                var elapsed = _dateTimeService.UtcNow - cycleStartedAt;
                var remaining = _cleanupSettings.Interval - elapsed;
                var delay = remaining > TimeSpan.Zero ? remaining : TimeSpan.Zero;
                await Task.Delay(delay, stoppingToken);
                return true;
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("PurgeExpiredTokensHostedService: cancelado durante la espera entre ciclos.");
                return false;
            }
        }
    }
}