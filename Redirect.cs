using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Cosmos.Table;
using System.Linq;
using Newtonsoft.Json;

namespace SirSuperGeek.AzFunc.ShortUrl
{
    public static class Redirect
    {
        

        [FunctionName("Redirect")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "{*all}")] HttpRequest req, ILogger log)
        {
            
            var badChars = "/\\".ToCharArray();
            string shortUrl = req.Path;
            shortUrl = shortUrl.TrimStart(badChars).TrimEnd(badChars);
            
            log.LogInformation(string.Format("Short URL requested for {0}, seeking row with key {1}", req.Path, shortUrl));

            var storageCreds = new StorageCredentials(Environment.GetEnvironmentVariable("AccountName"),Environment.GetEnvironmentVariable("AccountKey"));
            var storageAccount = new CloudStorageAccount(storageCreds, useHttps: true);
            var storageClient = storageAccount.CreateCloudTableClient();
            var storageTable = storageClient.GetTableReference(Environment.GetEnvironmentVariable("TableName"));
            
            var query = new TableQuery<Shortener>().Where(
                TableQuery.CombineFilters(
                    TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, Environment.GetEnvironmentVariable("PartitionKey")),
                    TableOperators.And,
                    TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, shortUrl)
                )
            );
            
            string redirectUrl;
            string defaultUrl = Environment.GetEnvironmentVariable("DefaultRedirect");
            var recordSet = storageTable.ExecuteQuery(query).ToList();

            if(recordSet.Count == 0) {
                log.LogInformation(String.Format("Table query returned no results, redirect/302ing to {0} (default)", defaultUrl));
                redirectUrl = defaultUrl;
            } else {
                log.LogInformation(string.Format("Table query returned {0}, redirect/302ing", recordSet[0].Url));
                redirectUrl = recordSet[0].Url;
            }

            return new RedirectResult(redirectUrl, false);
        }
    }
}
