using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RUNChecker.Services;
using RUNChecker.Options;
using Serilog;
using Microsoft.Extensions.Configuration;

namespace RUNChecker
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var envConfig = new ConfigurationBuilder()
               .AddEnvironmentVariables()
               .AddCommandLine(args)
               .Build();

            var hostedEnvironment = envConfig["DOTNET_ENVIRONMENT"];

            ConfigurationManager configuration = (ConfigurationManager)new ConfigurationManager()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{hostedEnvironment}.json", optional: true)
                .AddEnvironmentVariables();

            var builder = Host.CreateApplicationBuilder(new HostApplicationBuilderSettings()
            {
                Configuration = configuration
            });
            builder.Services
                .AddHostedService<RunCheckerSevice>()
                .AddSingleton<ServiceAccountChecker>()
                .AddSingleton<ServiceAccountUpdater>()
                .AddSingleton<CertificateChecker>()
                .AddSingleton<CertificateUpdater>()
                .AddSingleton<AzureBacklog>()
                .AddSingleton<EmailService>()
                .AddDbContextFactory<RunCheckerContext>()
                .AddTransient<SSLStream>()
                .Configure<CertificateCheckerOptions>(builder.Configuration.GetSection(CertificateCheckerOptions.CertificateChecker))
                .Configure<ServiceAccountCheckerOptions>(builder.Configuration.GetSection(ServiceAccountCheckerOptions.ServiceAccountChecker))
                .Configure<EmailServiceOptions>(builder.Configuration.GetSection(EmailServiceOptions.EmailService))
                .Configure<AzureOptions>(builder.Configuration.GetSection(AzureOptions.Azure))
                .Configure<ConnectionStringsOptions>(builder.Configuration.GetSection(ConnectionStringsOptions.ConnectionStrings))
                .AddSerilog(cfg => cfg.ReadFrom.Configuration(configuration), true);

            var host = builder.Build();
            host.Run();
        }
    }
}