using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RUNChecker.Options;

namespace RUNChecker.Services;

public class CertificateChecker(ILogger<CertificateChecker> logger, IDbContextFactory<RunCheckerContext> runCheckerContextFactory, SSLStream sslStream, IOptions<CertificateCheckerOptions> certificateCheckerOptions)
{
    private readonly ILogger<CertificateChecker> _logger = logger;
    private readonly IDbContextFactory<RunCheckerContext> _runCheckerContextFactory = runCheckerContextFactory;
    private readonly SSLStream _sslStream = sslStream;
    private readonly CertificateCheckerOptions _certificateCheckerOptions = certificateCheckerOptions.Value;

    public async Task Run()
    {
        if (_certificateCheckerOptions.CheckCertificates == true)
        {
            using (var ctx = _runCheckerContextFactory.CreateDbContext())
            {
                foreach (var app in ctx.Applications.Include(app => app.Certificates).ThenInclude(cert => cert.AppEnvironment).ToList())
                {
                    foreach (var cert in app.Certificates)
                    {
                        _sslStream.LastError = null;

                        CertificateProperties ? certificate = await _sslStream.Check(cert.HostName);
                        if (certificate != null)
                        {
                            // Update LastCheckedOn to current time
                            DateTimeOffset? currentDateTime = DateTime.Now;
                            cert.LastCheckedOn = currentDateTime;
                            cert.ErrorMessage = "";
                            cert.Error = false;
                            cert.Expiring = false;
                            cert.Expired = false;

                            if (certificate.CurrentExpiresOn < DateTime.Today) // Expired
                            {
                                cert.Expired = true;

                                // Calculate the number of days expired
                                int daysExpired = (DateTime.Today - certificate.CurrentExpiresOn).Days;
                                _logger.LogCritical($"URL: {cert.HostName} - APP: {app.Name} - ENV: {cert.AppEnvironment.Name}. Expired for {daysExpired} days");

                                // Update CurrentIssueOn to show that a problem was found
                                cert.CurrentIssueOn = currentDateTime;
                            }
                            else // Not Expired
                            {
                                // Calculate the remaining days until expiration
                                int remainingDays = (certificate.CurrentExpiresOn - DateTime.Today).Days;

                                // Check if it's closer than DaysRemainingToFlag
                                if (remainingDays < _certificateCheckerOptions.DaysRemainingToFlag)
                                {
                                    cert.Expiring = true;
                                    _logger.LogWarning($"URL: {cert.HostName} - APP: {app.Name} - ENV: {cert.AppEnvironment.Name}. Certificate will expire in less than {_certificateCheckerOptions.DaysRemainingToFlag} days ({remainingDays} days remaining).");

                                    // Update CurrentIssueOn to show that a problem was found
                                    cert.CurrentIssueOn = currentDateTime;
                                }
                                else // Expires later than 4 weeks
                                {
                                    _logger.LogInformation($"URL: {cert.HostName} - APP: {app.Name} - ENV: {cert.AppEnvironment.Name}. Certificate will expire in {remainingDays} days.");
                                }
                            }
                            // Update database with expire time. Need to convert to DateTimeOffset? for the database to take it.
                            DateTimeOffset? dateTimeOffset = certificate.CurrentExpiresOn;
                            cert.CurrentExpiresOn = dateTimeOffset;
                            cert.CurrentSubject = certificate.CurrentSubject;
                            cert.CurrentProtocol = certificate.CurrentProtocol;
                            cert.CurrentThumbprint = certificate.CurrentThumbprint;
                            ctx.SaveChanges();
                        }
                        else
                        {
                            // Add errors to backlog
                            cert.Error = true;
                            cert.ErrorMessage = _sslStream.LastError.Length > 250 ? _sslStream.LastError.Substring(0, 250) : _sslStream.LastError;

                            cert.CurrentThumbprint = null;
                            cert.CurrentSubject = null;
                            cert.CurrentProtocol = null;
                            cert.Expired = null;
                            cert.Expiring = null;
                            cert.CurrentExpiresOn = null;

                            ctx.SaveChanges();
                        }
                    }
                }
            }
        }
    }
}
