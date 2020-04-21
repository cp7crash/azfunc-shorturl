using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.WindowsAzure.Storage.Auth;
using Newtonsoft.Json;

namespace SirSuperGeek.AzFunc.ShortUrl
{
    public static class Redirect
    {
        

        [FunctionName("Redirect")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "{*all}")] HttpRequest req, ILogger log)
        {
            var storageUri = new Uri("core.windows.net");
            var storageCredentials = new StorageCredentials("ctlol","0CqrwMs0xTuwLJOZqJiWStxsCFKUbpSHF5WLTkXtg9w7Zgefz52Uvd8CoD71AcWlunjCqB6yccEXeBDKebHuQw==");
            var tableClient = new CloudTableClient(storageUri, storageCredentials);
                        
            string shortUrl = req.Path;
            var result = new TableQuery<Shortener>().Where(
                TableQuery.CombineFilters(
                    TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, 
                        Environment.GetEnvironmentVariable("PartitionKey")),
                    TableOperators.And,
                    TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.GreaterThan, 
                        shortUrl)));

            string redirectUrl = "nope";

            log.LogInformation(string.Format("Function called for /{0}", shortUrl));

            // fetch default key if miss

            // redirect to ct.com otherwise

            // 302 redirect
            return new RedirectResult(redirectUrl, false);
        }
    }
}
