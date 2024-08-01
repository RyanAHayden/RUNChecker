using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RUNChecker.Options;
using System.Runtime.ConstrainedExecution;

namespace RUNChecker.Services
{
    public class ServiceAccountUpdater(ILogger<CertificateChecker> logger, EmailService emailService, AzureBacklog azureBacklog, IOptions<AzureOptions> azureOptions, IDbContextFactory<RunCheckerContext> runCheckerContextFactory, IOptions<ServiceAccountCheckerOptions> serviceAccountCheckerOptions)
    {
        private readonly EmailService _emailService = emailService;
        private readonly AzureBacklog _azureBacklog = azureBacklog;
        private readonly AzureOptions _azureOptions = azureOptions.Value;

        private readonly ILogger<CertificateChecker> _logger = logger;
        private readonly IDbContextFactory<RunCheckerContext> _runCheckerContextFactory = runCheckerContextFactory;
        private readonly ServiceAccountCheckerOptions _serviceAccountCheckerOptions = serviceAccountCheckerOptions.Value;

        public async Task Run()
        {
            if (_serviceAccountCheckerOptions.CheckServiceAccounts == true)
            {
                // HTML Table Backlog Item
                string tableHeadEmail = "<html><style>table, th, td {border:1px solid black;}</style><table style=\"width:100%\"><tr style=\"background-color:LightGray;\"><th>App</th><th>Environment</th><th>Account Name</th><th>Employee Type</th><th>Expire Date</th><th>Last Logon</th><th>Error?</th><th>Error Message</th><th>Expiring?</th><th>Expired?</th><th>Backlog Item</th></tr>";
                string tableHead = "<html><style>table, th, td {border:1px solid black;}</style><table style=\"width:100%\"><tr style=\"background-color:LightGray;\"><th>App</th><th>Environment</th><th>Account Name</th><th>Employee Type</th><th>Expire Date</th><th>Last Logon</th><th>Error?</th><th>Error Message</th><th>Expiring?</th><th>Expired?</th></tr>";
                string tableEnd = "</table>";

                // Temp Backlog Item
                string tempTitle = "RUNChecker: Temporary Backlog Item";
                string tempDesc = "This was created by RUNChecker and it will be overwritten. <br>If you're seeing this it is because the application was ended prematurely.";

                List<string> tableRows = [];
                List<string> emailRows = [];

                if (_serviceAccountCheckerOptions.CheckServiceAccounts == true)
                {
                    var emailFlag = false;
                    bool errorFlag = false; // For adding errors to the backlog/email

                    using (var ctx = _runCheckerContextFactory.CreateDbContext())
                    {
                        foreach (var app in ctx.Applications.Include(app => app.ServiceAccounts).ThenInclude(acc => acc.AppEnvironment).ToList())
                        {
                            if (app.ServiceAccounts.Count < 0) // If there's no apps in a category skip
                            {
                                continue;
                            }

                            string projectName = Query.GetArea(ctx, app.Name, area => area.ProjectName);
                            string teamName = Query.GetArea(ctx, app.Name, area => area.BacklogTeam);
                            string areaName = Query.GetArea(ctx, app.Name, area => area.AreaName);
                            string iterationPath = Query.GetArea(ctx, app.Name, area => area.IterationPath);

                            bool expireFlag = false; // Flags the whole category to create backlogitem
                            bool currentItemFlag = false; // Flag for this specific account
                            bool expired = false; // Sets the specific account as expired

                            int priority = _serviceAccountCheckerOptions.DefaultPriority;

                            // This checks if any of the Accounts in the Application have errors, expired or expiring soon, then to mark the category to be put in an email/backlog
                            foreach (var accCheck in app.ServiceAccounts)
                            {
                                if (accCheck.Error == true || accCheck.Expired == true || accCheck.Expiring == true)
                                {
                                    expireFlag = true;
                                }
                            }
                            foreach (var acc in app.ServiceAccounts)
                            {
                                if (acc.Error == true)
                                {
                                    errorFlag = true;
                                }
                                else if (acc.Expired == true)
                                {
                                    expired = true;
                                    currentItemFlag = true;
                                    priority = _serviceAccountCheckerOptions.ExpiredPriority;
                                }
                                else if (acc.Expiring == true)
                                {
                                    currentItemFlag = true;
                                    priority = _serviceAccountCheckerOptions.ExpiringPriority;
                                }
                                // For each account
                                if (expireFlag || _serviceAccountCheckerOptions.AlwaysCreateBacklog)
                                {
                                    bool createFlag = false;
                                    bool editFlag = false;

                                    if (app.CurrentWorkItemAcc != null)
                                    {
                                        int lastItem = (int)app.CurrentWorkItemAcc;

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
                                                _azureBacklog.AccLastItem = lastItem;
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

                                    if (createFlag == true)
                                    {
                                        _azureBacklog.CreateBacklogItem(projectName, areaName, iterationPath, tempTitle, tempDesc, priority, 2);
                                    }
                                    backlogItem = _azureBacklog.AccLastItem.ToString();
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

                                    string rowString = $"<{row}><td>{app.Name}</td><td>{acc.AppEnvironment.Name}</td><td>{acc.AccountName}</td><td>{acc.EmployeeType}</td><td>{acc.CurrentExpiresOn}</td><td>{acc.LastLogon}</td><td>{acc.Error}</td><td>{acc.ErrorMessage}</td><td>{acc.Expiring}</td><td>{acc.Expired}</td></tr>";
                                    tableRows.Add(rowString);

                                    if (createFlag == true)
                                    {
                                        // Handle changing title to error or expire date.
                                        var earliestExpireDate = app.ServiceAccounts.Min(acc => acc.CurrentExpiresOn)?.ToString("MM/dd/yy") ?? "ERROR";
                                        var errorHandle = acc.Error.GetValueOrDefault(true) ? "ERROR" : earliestExpireDate;
                                        if (errorHandle == "ERROR" && earliestExpireDate != "ERROR")
                                        {
                                            errorHandle = errorHandle + " - " + earliestExpireDate;
                                            priority = _serviceAccountCheckerOptions.ExpiredPriority;
                                        }

                                        // Edit the newly created item
                                        try
                                        {
                                            _azureBacklog.EditBacklogItemAsync(_azureBacklog.AccLastItem, $"{app.Name} Account Expiration Info - {errorHandle}", tableHead + String.Join("", tableRows) + tableEnd, priority);
                                        }
                                        catch (AggregateException ex)
                                        {
                                            _logger.LogCritical($"Did not update {app.Name}: {ex}");
                                        }
                                        app.CurrentWorkItemAcc = _azureBacklog.AccLastItem; // This is required to not keep creating backlog items
                                        ctx.SaveChanges();
                                    }

                                    string rowStringEmail = $"<{row}><td>{app.Name}</td><td>{acc.AppEnvironment.Name}</td><td>{acc.AccountName}</td><td>{acc.EmployeeType}</td><td>{acc.CurrentExpiresOn}</td><td>{acc.LastLogon}</td><td>{acc.Error}</td><td>{acc.ErrorMessage}</td><td>{acc.Expiring}</td><td>{acc.Expired}</td><td><a href={backlogItemLink}>{backlogItem}</a></td></tr>";
                                    emailRows.Add(rowStringEmail);

                                    if (editFlag == true)
                                    {
                                        // Handle changing title to error or expire date.
                                        var earliestExpireDate = app.ServiceAccounts.Min(acc => acc.CurrentExpiresOn)?.ToString("MM/dd/yy") ?? "ERROR";
                                        var errorHandle = acc.Error.GetValueOrDefault(true) ? "ERROR" : earliestExpireDate;
                                        if (errorHandle == "ERROR" && earliestExpireDate != "ERROR")
                                        {
                                            errorHandle = errorHandle + " - " + earliestExpireDate;
                                            priority = _serviceAccountCheckerOptions.ExpiredPriority;
                                        }
                                        try
                                        {
                                            _azureBacklog.EditBacklogItemAsync(_azureBacklog.AccLastItem, $"{app.Name} Account Expiration Info - {errorHandle}", tableHead + String.Join("", tableRows) + tableEnd, priority);
                                        }
                                        catch (AggregateException ex)
                                        {
                                            _logger.LogCritical($"Did not update {app.Name}: {ex}");
                                        }
                                    }
                                    emailFlag = true; // Flag to send email
                                    currentItemFlag = false; // Reset flag for this specific account
                                    expired = false; // Reset expired flag
                                    errorFlag = false; // Reset error flag

                                    _logger.LogInformation($"Account Updated: {acc.AccountName}");

                                    // Reorder backlog
                                    if (createFlag)
                                    {
                                        _azureBacklog.ReorderBacklogAsync(_azureBacklog.AccLastItem, projectName, teamName);
                                    }
                                }
                            }
                            if (expireFlag || _serviceAccountCheckerOptions.AlwaysCreateBacklog)
                            {
                                priority = _serviceAccountCheckerOptions.DefaultPriority; // reset priority
                                tableRows.Clear(); // Gets rid of existing table rows for next account check
                            }
                        }
                        if (emailFlag)
                        {
                            //var earliestExpireDate = app.ServiceAccounts.Min(acc => acc.CurrentExpiresOn)?.ToString() ?? "ERROR";
                            // can't access earliest date from out here.
                            // emails will send with the current date.
                            _emailService.SendEmail($"Account Expiration Info - {DateTime.Now}", tableHeadEmail + String.Join("", emailRows) + tableEnd);
                        }
                    }
                }
            }
        }
    }
}
