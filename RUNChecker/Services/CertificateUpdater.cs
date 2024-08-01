using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RUNChecker.Options;
using System.Runtime.ConstrainedExecution;

namespace RUNChecker.Services
{
    public class CertificateUpdater(ILogger<CertificateChecker> logger, IDbContextFactory<RunCheckerContext> runCheckerContextFactory, EmailService emailService, AzureBacklog azureBacklog, IOptions<CertificateCheckerOptions> certificateCheckerOptions, IOptions<AzureOptions> azureOptions)
    {
        private readonly ILogger<CertificateChecker> _logger = logger;
        private readonly IDbContextFactory<RunCheckerContext> _runCheckerContextFactory = runCheckerContextFactory;
        private readonly AzureBacklog _azureBacklog = azureBacklog;
        private readonly CertificateCheckerOptions _certificateCheckerOptions = certificateCheckerOptions.Value;
        private readonly EmailService _emailService = emailService;
        private readonly AzureOptions _azureOptions = azureOptions.Value;

        public async Task Run()
        {
            if (_certificateCheckerOptions.CheckCertificates == true)
            {
                // HTML Table Backlog Item
                string tableHeadEmail = "<html><style>table, th, td {border:1px solid black;}</style><table style=\"width:100%\"><tr style=\"background-color:LightGray;\"><th>App</th><th>Environment</th><th>Host Name</th><th>Subject</th><th>Expire Date</th><th>Thumbprint</th><th>Protocol</th><th>Error?</th><th>Error Message</th><th>Expiring?</th><th>Expired?</th><th>Backlog Item</th></tr>";
                string tableHead = "<html><style>table, th, td {border:1px solid black;}</style><table style=\"width:100%\"><tr style=\"background-color:LightGray;\"><th>App</th><th>Environment</th><th>Host Name</th><th>Subject</th><th>Expire Date</th><th>Thumbprint</th><th>Protocol</th><th>Error?</th><th>Error Message</th><th>Expiring?</th><th>Expired?</th></tr>";
                string tableEnd = "</table>";

                // Temp Backlog Item
                string tempTitle = "RUNChecker: Temporary Backlog Item";
                string tempDesc = "This was created by RUNChecker and it will be overwritten. <br>If you're seeing this it is because the application was ended prematurely.";

                List<string> tableRows = [];
                List<string> emailRows = [];

                using (var ctx = _runCheckerContextFactory.CreateDbContext())
                {
                    var emailFlag = false;
                    bool errorFlag = false; // For adding errors to the backlog/email

                    foreach (var app in ctx.Applications.Include(app => app.Certificates).ThenInclude(cert => cert.AppEnvironment).ToList())
                    {
                        string projectName = Query.GetArea(ctx, app.Name, area => area.ProjectName);
                        string teamName = Query.GetArea(ctx, app.Name, area => area.BacklogTeam);
                        string areaName = Query.GetArea(ctx, app.Name, area => area.AreaName);
                        string iterationPath = Query.GetArea(ctx, app.Name, area => area.IterationPath);

                        bool expireFlag = false; // Flags the whole category to create backlogitem
                        bool currentItemFlag = false; // Flag for this specific certificate
                        bool expired = false; // Sets the specific certificate as expired
                        int priority = _certificateCheckerOptions.DefaultPriority;

                        // This checks if any of the Certificates in the Application have errors, expired or expiring soon, then to mark the category to be put in an email/backlog
                        foreach (var certCheck in app.Certificates)
                        {
                            if (certCheck.Error == true || certCheck.Expired == true || certCheck.Expiring == true)
                            {
                                expireFlag = true;
                            }
                        }

                        foreach (var cert in app.Certificates)
                        {
                            if (cert.Error == true)
                            {
                                errorFlag = true;
                            }
                            else if (cert.Expired == true)
                            {
                                expired = true;
                                currentItemFlag = true;
                                priority = _certificateCheckerOptions.ExpiredPriority;
                            }
                            else if (cert.Expiring == true)
                            {
                                currentItemFlag = true;
                                priority = _certificateCheckerOptions.ExpiringPriority;
                            }
                            // For each certificate
                            if (expireFlag || _certificateCheckerOptions.AlwaysCreateBacklog)
                            {
                                bool createFlag = false;
                                bool editFlag = false;

                                if (app.CurrentWorkItemCert != null)
                                {
                                    int lastItem = (int)app.CurrentWorkItemCert;

                                    string? state = await _azureBacklog.GetWorkItemStateAsync(lastItem);
                                    if (state != null)
                                    {
                                        //_logger.LogInformation($"State of {lastItem}: {state}");
                                        if (state == "Done" || state == "Removed")
                                        {
                                            createFlag = true;
                                        }
                                        else
                                        {
                                            editFlag = true;
                                            _azureBacklog.CertLastItem = lastItem;
                                        }
                                    }
                                    else
                                    {
                                        createFlag = true;
                                    }
                                }
                                else
                                {
                                    createFlag = true;
                                }

                                string row = "tr";
                                string backlogItem = "N/A";
                                string backlogItemLink = "";



                                // Create backlog item so we can get ID for the email
                                if (createFlag)
                                {
                                    _logger.LogInformation($"Backlog Item Created. ID: {
                                        _azureBacklog.CreateBacklogItem(projectName, areaName, iterationPath, tempTitle, tempDesc, priority)
                                        }");
                                }
                                backlogItem = _azureBacklog.CertLastItem.ToString();
                                string projectNameUrlEncoded = projectName.Replace(" ", "%20");
                                backlogItemLink = $"{_azureOptions.URL}/{projectNameUrlEncoded}/_workitems/edit/{backlogItem}";

                                if (expired) // Highlights red if account is expired
                                {
                                    row = "tr style=\"background-color:red;\"";
                                }
                                else if (currentItemFlag) // Highlights yellow if account is within DaysRemainingToFlag
                                {
                                    row = "tr style=\"background-color:yellow\"";
                                }
                                else if (errorFlag)
                                {
                                    row = "tr style=\"background-color:tomato;\""; // Highlights tomato if error
                                }

                                string rowString = $"<{row}><td>{app.Name}</td><td>{cert.AppEnvironment.Name}</td><td>{cert.HostName}</td><td>{cert.CurrentSubject}</td><td>{cert.CurrentExpiresOn}</td><td>{cert.CurrentThumbprint}</td><td>{cert.CurrentProtocol}</td><td>{cert.Error}</td><td>{cert.ErrorMessage}</td><td>{cert.Expiring}</td><td>{cert.Expired}</td></tr>";
                                tableRows.Add(rowString);

                                if (createFlag == true)
                                {
                                    // Handle changing title to error or expire date.
                                    var earliestExpireDate = app.Certificates.Min(cert => cert.CurrentExpiresOn)?.ToString("MM/dd/yy") ?? "ERROR";
                                    var errorHandle = cert.Error.GetValueOrDefault(true) ? "ERROR" : earliestExpireDate;
                                    if (errorHandle == "ERROR" && earliestExpireDate != "ERROR")
                                    {
                                        errorHandle = errorHandle + " - " + earliestExpireDate;
                                        priority = _certificateCheckerOptions.ExpiredPriority;
                                    }
                                    // Edit the newly created item
                                    try
                                    {
                                        _azureBacklog.EditBacklogItemAsync(_azureBacklog.CertLastItem, $"{app.Name} Certificate Expiration Info - {errorHandle}", tableHead + String.Join("", tableRows) + tableEnd, priority);
                                    }
                                    catch (AggregateException ex)
                                    {
                                        _logger.LogCritical($"Did not update {app.Name}: {ex}");
                                    }
                                    app.CurrentWorkItemCert = _azureBacklog.CertLastItem; // This is required to not keep creating backlog items
                                    ctx.SaveChanges();

                                    // Reorder backlog
                                    if (createFlag)
                                    {
                                        _azureBacklog.ReorderBacklogAsync(_azureBacklog.CertLastItem, projectName, teamName);
                                    }
                                }

                                string rowStringEmail = $"<{row}><td>{app.Name}</td><td>{cert.AppEnvironment.Name}</td><td>{cert.HostName}</td><td>{cert.CurrentSubject}</td><td>{cert.CurrentExpiresOn}</td><td>{cert.CurrentThumbprint}</td><td>{cert.CurrentProtocol}</td><td>{cert.Error}</td><td>{cert.ErrorMessage}</td><td>{cert.Expiring}</td><td>{cert.Expired}</td><td><a href={backlogItemLink}>{backlogItem}</a></td></tr>";
                                emailRows.Add(rowStringEmail);

                                if (editFlag == true)
                                {
                                    // Handle changing title to error or expire date.
                                    var earliestExpireDate = app.Certificates.Min(cert => cert.CurrentExpiresOn)?.ToString("MM/dd/yy") ?? "ERROR";
                                    var errorHandle = cert.Error.GetValueOrDefault(true) ? "ERROR" : earliestExpireDate;
                                    if (errorHandle == "ERROR" && earliestExpireDate != "ERROR")
                                    {
                                        errorHandle = errorHandle + " - " + earliestExpireDate;
                                        priority = _certificateCheckerOptions.ExpiredPriority;
                                    }

                                    try
                                    {
                                        _azureBacklog.EditBacklogItemAsync(_azureBacklog.CertLastItem, $"{app.Name} Certificate Expiration Info - {errorHandle}", tableHead + String.Join("", tableRows) + tableEnd, priority);
                                    }
                                    catch (AggregateException ex)
                                    {
                                        _logger.LogCritical($"Did not update {app.Name}: {ex}");
                                    }
                                }
                                emailFlag = true; // Flag to send email
                                currentItemFlag = false; // Reset flag for this specific certificate
                                expired = false; // Reset expired flag
                                errorFlag = false; // Reset error flag

                                _logger.LogInformation($"Certificate Updated: {cert.HostName}");
                            }
                        }
                        if (expireFlag || _certificateCheckerOptions.AlwaysCreateBacklog)
                        {
                            priority = _certificateCheckerOptions.DefaultPriority; // reset priority
                            tableRows.Clear(); // Gets rid of existing table rows for next application check
                        }
                    }
                    if (emailFlag)
                    {
                        //var earliestExpireDate = app.ServiceAccounts.Min(acc => acc.CurrentExpiresOn)?.ToString() ?? "ERROR";
                        // can't access earliest date from out here.
                        // TODO
                        _emailService.SendEmail($"Certificate Expiration Info - {DateTime.Now}", tableHeadEmail + String.Join("", emailRows) + tableEnd);
                    }
                }
            }
        }
    }
}