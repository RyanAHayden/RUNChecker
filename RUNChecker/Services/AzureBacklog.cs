using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.TeamFoundation.Core.WebApi.Types;
using Microsoft.TeamFoundation.Work.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.Organization.Client;
using Microsoft.VisualStudio.Services.WebApi;
using Microsoft.VisualStudio.Services.WebApi.Patch;
using Microsoft.VisualStudio.Services.WebApi.Patch.Json;
using RUNChecker.Options;
using System.Net.Http.Headers;
using System.Text;

namespace RUNChecker.Services
{
    public class AzureBacklog(ILogger<CertificateChecker> logger, IOptions<AzureOptions> azureOptions)
    {

        private readonly ILogger<CertificateChecker> _logger = logger;
        private readonly AzureOptions _azureOptions = azureOptions.Value;

        public int CertLastItem { get; set; }
        public int AccLastItem { get; set; }

        // Put the backlog item at the top
        public async Task ReorderBacklogAsync(int backlogItem, string projectName, string teamName)
        {
            var workClient = new Microsoft.TeamFoundation.Work.WebApi.WorkHttpClient(new Uri(_azureOptions.URL), new VssBasicCredential("RUNChecker", _azureOptions.AccessToken));
            int workItemNumber = backlogItem;
            var reorderOperation = new Microsoft.TeamFoundation.Work.WebApi.ReorderOperation()
            {
                Ids = new[] { workItemNumber },
                ParentId = null,
                IterationPath = null,
                NextId = null,
                PreviousId = 0
            };
            var teamContext = new Microsoft.TeamFoundation.Core.WebApi.Types.TeamContext(projectName, teamName);
            await workClient.ReorderBacklogWorkItemsAsync(reorderOperation, teamContext);
        }
        public int CreateBacklogItem(string projectName, string areaName, string iterationPath, string title, string description, int priority, int type = 1)
        {
            // Connect to Azure DevOps
            VssConnection connection = new(new Uri(_azureOptions.URL), new VssBasicCredential("RUNChecker", _azureOptions.AccessToken));

            // Get the WorkItemTrackingHttpClient
            WorkItemTrackingHttpClient witClient = connection.GetClient<WorkItemTrackingHttpClient>();

            // backlog fields
            JsonPatchDocument fields =
                [
                    new JsonPatchOperation { Operation = Operation.Add, Path = "/fields/System.Title", Value = title },
                    new JsonPatchOperation { Operation = Operation.Add, Path = "/fields/System.Description", Value = description },
                    new JsonPatchOperation { Operation = Operation.Add, Path = "/fields/System.AreaPath", Value = projectName + "\\" + areaName },
                    new JsonPatchOperation { Operation = Operation.Add, Path = "/fields/System.IterationPath", Value = projectName + "\\" + iterationPath},
                    new JsonPatchOperation { Operation = Operation.Add, Path = "/fields/Microsoft.VSTS.Common.Priority", Value = priority }
                ];

            // Create the work item
            var createItemTask = witClient.CreateWorkItemAsync(fields, projectName, "Product Backlog Item");
            createItemTask.Wait();
            var result = createItemTask.Result;

            // Set and display the created backlog item ID
            if (result.Id != null)
            {
                switch (type)
                {
                    case 1:
                        CertLastItem = (int)result.Id;
                        return CertLastItem;
                    case 2:
                        AccLastItem = (int)result.Id;
                        return AccLastItem;
                    default:
                        return 0;
                }
            }
            else
            {
                return 0;
            }
        }

    public async Task EditBacklogItemAsync(int backlogItemId, string title, string description, int priority)
    {
        // Connect to Azure DevOps
        VssConnection connection = new(new Uri(_azureOptions.URL), new VssBasicCredential("RUNChecker", _azureOptions.AccessToken));

        // Get the WorkItemTrackingHttpClient
        WorkItemTrackingHttpClient witClient = connection.GetClient<WorkItemTrackingHttpClient>();

        // Update fields
        JsonPatchDocument fields =
            [
                new JsonPatchOperation { Operation = Operation.Replace, Path = "/fields/System.Title", Value = title },
                    new JsonPatchOperation { Operation = Operation.Replace, Path = "/fields/System.Description", Value = description },
                    new JsonPatchOperation { Operation = Operation.Replace, Path = "/fields/Microsoft.VSTS.Common.Priority", Value = priority }
            ];

            // Update the work item
            _ = witClient.UpdateWorkItemAsync(fields, backlogItemId).Result;


            // Display the updated backlog item ID
            _logger.LogInformation($"Backlog Item Updated. ID: {backlogItemId}");
    }

    public async Task<string?> GetWorkItemStateAsync(int workItemId)
    {
        try
        {
            VssConnection connection = new VssConnection(new Uri(_azureOptions.URL), new VssBasicCredential(string.Empty, _azureOptions.AccessToken));
            WorkItemTrackingHttpClient witClient = connection.GetClient<WorkItemTrackingHttpClient>();

            var workItem = await witClient.GetWorkItemAsync(workItemId, expand: WorkItemExpand.Relations | WorkItemExpand.Fields);

            // Check if the work item is not null and contains the 'State' field
            if (workItem != null && workItem.Fields.ContainsKey("System.State"))
            {
                // Get the value of the 'State' field
                var state = workItem.Fields["System.State"];

                return state.ToString();
            }
            else
            {
                _logger.LogError("State field not found");
                return null;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning($"GetWorkItemStateAsync: {ex.Message}");
            return null;
        }
    }
}
}