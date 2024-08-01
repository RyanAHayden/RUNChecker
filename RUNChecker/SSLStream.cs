using Microsoft.Extensions.Logging;
using RUNChecker.Services;
using System.Globalization;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;

namespace RUNChecker
{
    public class SSLStream(ILogger<CertificateChecker> logger)
    {
        private readonly ILogger<CertificateChecker> _logger = logger;
        private const int port = 443;

        public string LastError { get; set; }

        public async Task<CertificateProperties?> Check(string url)
        {
            if (url != string.Empty)
            {
                try
                {
                    using TcpClient client = new(url, port);
                    using SslStream sslStream = new(client.GetStream(), false, (sender, certificate, chain, sslPolicyErrors) => true);
                    await sslStream.AuthenticateAsClientAsync(url);

                    if (sslStream.RemoteCertificate != null)
                    {
                        X509Certificate certificate = sslStream.RemoteCertificate;

                        if (certificate != null)
                        {
                            DateTime expireDateTime = DateTime.ParseExact(certificate.GetExpirationDateString(), "M/d/yyyy h:mm:ss tt", CultureInfo.InvariantCulture);
                            DateTimeOffset? currentDateTime = DateTime.Now;
                            CertificateProperties cert = new()
                            {
                                HostName = url,
                                CurrentSubject = certificate.Subject,
                                CurrentExpiresOn = expireDateTime,
                                CurrentThumbprint = certificate.GetCertHashString(),
                                CurrentProtocol = sslStream.SslProtocol.ToString(),
                                CurrentIssueOn = currentDateTime
                            };
                            return cert;
                        }
                        else
                        {
                            LastError = $"No SSL certificate information available for: {url}";
                            _logger.LogError(LastError);
                        }
                        return null;
                    }
                    else
                    {
                        LastError = $"SSL certificate is null for: {url}";
                        _logger.LogError(LastError);
                    }
                    return null;
                }
                catch (Exception ex)
                {
                    LastError = $"Unknown: {url}: {ex.Message}";
                    _logger.LogError(LastError);
                    return null;
                }
            }
            else
            {
                LastError = $"No URL given for: {url}";
                _logger.LogError(LastError);
                return null;
            }
        }
    }
}