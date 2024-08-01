using Microsoft.Extensions.Hosting;

namespace RUNChecker.Services
{
    public class RunCheckerSevice(CertificateChecker certificateChecker, CertificateUpdater certificateUpdater, ServiceAccountChecker serviceAccountChecker, ServiceAccountUpdater serviceAccountUpdater, IHostApplicationLifetime hostApplicationLifetime) : BackgroundService
    {
        private readonly IHostApplicationLifetime _applicationLifetime = hostApplicationLifetime;
        private readonly CertificateChecker _certificateChecker = certificateChecker;
        private readonly CertificateUpdater _certificateUpdater = certificateUpdater;
        private readonly ServiceAccountChecker _serviceAccountChecker = serviceAccountChecker;
        private readonly ServiceAccountUpdater _serviceAccountUpdater = serviceAccountUpdater;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await _serviceAccountChecker.Run();
            await _serviceAccountUpdater.Run();
            await _certificateChecker.Run();
            await _certificateUpdater.Run();
            _applicationLifetime.StopApplication();
        }
    }
}
