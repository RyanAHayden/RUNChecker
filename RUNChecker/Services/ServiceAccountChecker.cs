using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RUNChecker.Options;

namespace RUNChecker.Services
{
    public class ServiceAccountChecker(ILogger<CertificateChecker> logger, IDbContextFactory<RunCheckerContext> runCheckerContextFactory, IOptions<ServiceAccountCheckerOptions> serviceAccountCheckerOptions)
    {
        private readonly ILogger<CertificateChecker> _logger = logger;
        private readonly IDbContextFactory<RunCheckerContext> _runCheckerContextFactory = runCheckerContextFactory;
        private readonly ServiceAccountCheckerOptions _serviceAccountCheckerOptions = serviceAccountCheckerOptions.Value;

        public async Task Run()
        {
            if (_serviceAccountCheckerOptions.CheckServiceAccounts == true)
            {
                using (var ctx = _runCheckerContextFactory.CreateDbContext())
                {
                    foreach (var app in ctx.Applications.Include(app => app.ServiceAccounts).ThenInclude(acc => acc.AppEnvironment).ToList())
                    {
                        string foundEmployeeType = "";
                        DateTime? lastLogon = DateTime.MinValue;
                        DateTime? passwordLastSet = DateTime.MinValue;
                        string foundUsername = "";

                        // If no accounts are under the app category, skip.
                        if (app.ServiceAccounts.Count < 1)
                        {
                            continue;
                        }
                        using (PrincipalContext context = new(ContextType.Domain, "thadmin.com"))
                        {
                            foreach (var acc in app.ServiceAccounts)
                            {
                                using (PrincipalSearcher searcher = new(new UserPrincipal(context) { SamAccountName = acc.AccountName }))
                                {
                                    var searchResult = searcher.FindOne();

                                    if (searchResult == null)
                                    {
                                        // Account not found
                                        _logger.LogError($"AccountChecker: User '{acc.AccountName}' not found.");
                                        acc.Error = true;
                                        acc.ErrorMessage = $"AccountChecker: User '{acc.AccountName}' not found.";

                                        acc.EmployeeType = null;
                                        acc.CurrentExpiresOn = null;
                                        acc.LastLogon = null;
                                        acc.Expiring = null;
                                        acc.Expired = null;

                                        ctx.SaveChanges();
                                    }
                                    else
                                    {
                                        acc.Error = false;

                                        DirectoryEntry directoryEntry = searchResult.GetUnderlyingObject() as DirectoryEntry;
                                        if (directoryEntry != null)
                                        {
                                            passwordLastSet = ((UserPrincipal)searchResult).LastPasswordSet;
                                            foundUsername = ((UserPrincipal)searchResult).SamAccountName;

                                            lastLogon = ((UserPrincipal)searchResult).LastLogon;

                                            foundEmployeeType = directoryEntry.Properties["employeeType"].Value.ToString();
                                            bool? isEnabled = ((UserPrincipal)searchResult).Enabled;

                                            // Update when the password was set (LastPasswordSet)
                                            DateTimeOffset? lastSet = passwordLastSet.Value;
                                            acc.LastPasswordSet = lastSet;

                                            // Update expected expire date (CurrentExpiresOn)
                                            DateTimeOffset? expiresOn = lastSet.Value.AddDays(acc.DaysToExpire);
                                            acc.CurrentExpiresOn = expiresOn;

                                            // Update LastCheckedOn
                                            DateTimeOffset? currentDateTime = DateTime.Now;
                                            acc.LastCheckedOn = currentDateTime;

                                            // Update LastLogon
                                            acc.LastLogon = lastLogon;

                                            // Update EmployeeType
                                            acc.EmployeeType = foundEmployeeType;

                                            if (acc.LastPasswordSet != null)
                                            {
                                                TimeSpan timeSinceLastPasswordChange = DateTime.Now - acc.LastPasswordSet.Value;
                                                int numberOfDays = (int)timeSinceLastPasswordChange.TotalDays;
                                                var daysLeft = acc.DaysToExpire - numberOfDays;

                                                acc.Expired = false;
                                                acc.Expiring = false;

                                                if (daysLeft < 0)
                                                {
                                                    _logger.LogCritical($"ACC: {acc.AccountName} - APP: {app.Name} - ENV: {acc.AppEnvironment.Name}. Expired for {-daysLeft} days");
                                                    acc.Expired = true;
                                                }
                                                else if (daysLeft < _serviceAccountCheckerOptions.DaysRemainingToFlag)
                                                {
                                                    _logger.LogWarning($"ACC: {acc.AccountName} - APP: {app.Name} - ENV: {acc.AppEnvironment.Name}. Expiring soon: {daysLeft} days left");
                                                    acc.Expiring = true;
                                                }
                                                else
                                                {
                                                    _logger.LogInformation($"ACC: {acc.AccountName} - APP: {app.Name} - ENV: {acc.AppEnvironment.Name}. Expiring in {daysLeft} days");
                                                }
                                            }
                                            ctx.SaveChanges();
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}