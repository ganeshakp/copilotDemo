using Newtonsoft.Json; // Used for JSON parsing and serialization
using RestSharp; // Used for sending HTTP requests
using System; // Provides fundamental classes and base classes
using System.IO; // Used for file handling

namespace SeleniumTests
{
    /// <summary>
    /// A utility class for creating bugs in Azure DevOps.
    /// </summary>
    public class AzureDevOpsBugCreator
    {
        private string azureDevOpsUrl; // Azure DevOps base URL
        private string project; // Azure DevOps project name
        private string personalAccessToken; // Azure DevOps personal access token for authentication

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureDevOpsBugCreator"/> class.
        /// Reads configuration from the appsettings.json file.
        /// </summary>
        public AzureDevOpsBugCreator()
        {
            try
            {
                var config = File.ReadAllText("appsettings.json");
                dynamic settings = JsonConvert.DeserializeObject(config);

                // Fetch configuration settings
                azureDevOpsUrl = settings.AzureDevOpsUrl 
                    ?? throw new Exception("AzureDevOpsUrl is not provided in appsettings.json");
                project = settings.Project 
                    ?? throw new Exception("Project is not provided in appsettings.json");
                personalAccessToken = settings.PersonalAccessToken 
                    ?? throw new Exception("PersonalAccessToken is not provided in appsettings.json");

            }
            catch (FileNotFoundException ex)
            {
                Console.Error.WriteLine("Configuration file not found: " + ex.Message);
                throw; // Re-throw the exception for the caller to handle
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("Failed to initialize AzureDevOpsBugCreator: " + ex.Message);
                throw; // Re-throw the exception for the caller to handle
            }
        }

        /// <summary>
        /// Creates a bug in Azure DevOps with the specified title.
        /// </summary>
        /// <param name="bugTitle">The title of the bug to create.</param>
        public void CreateBug(string bugTitle)
        {
            try
            {
                var client = new RestClient($"{azureDevOpsUrl}/{project}/_apis/wit/workitems/$Bug?api-version=6.0");
                var request = new RestRequest(Method.POST);

                // Add headers
                request.AddHeader("Content-Type", "application/json-patch+json");
                string authToken = Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes($":{personalAccessToken}"));
                request.AddHeader("Authorization", $"Basic {authToken}");

                // Prepare the bug data
                var bugData = new[]
                {
                    new { op = "add", path = "/fields/System.Title", value = bugTitle },
                    new { op = "add", path = "/fields/System.Description", value = "Bug created automatically due to failed Selenium test." },
                    new { op = "add", path = "/fields/System.AssignedTo", value = "shahab@tecoholic.com" },
                    new { op = "add", path = "/fields/Microsoft.VSTS.TCM.ReproSteps", value = "See attached logs for detailed error." }
                };

                // Serialize and add bug data in request body
                request.AddParameter("application/json-patch+json", JsonConvert.SerializeObject(bugData), ParameterType.RequestBody);

                // Execute the request
                IRestResponse response = client.Execute(request);

                // Handle response
                if (response.IsSuccessful)
                {
                    Console.WriteLine("Bug successfully created in Azure DevOps.");
                }
                else
                {
                    Console.Error.WriteLine($"Bug creation failed. HTTP Status Code: {response.StatusCode} | Error: {response.Content}");
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("An error occurred while creating the bug: " + ex.Message);
                throw; // Re-throw for further handling if necessary
            }
        }
    }
}
