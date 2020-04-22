using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Cosmos.Table;

namespace SirSuperGeek.AzFunc.ShortUrl
{
    public static class Shortener
    {
        
        static MemoryCache urlCache = new MemoryCache(new MemoryCacheOptions());

        [FunctionName("Shortener")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "{*all}")] HttpRequest req, ILogger log)
        {
            
            string defaultUrl = Environment.GetEnvironmentVariable("DefaultRedirect");
            string redirectUrl;

            var badChars = "/\\".ToCharArray();
            string shortUrl = req.Path;
            shortUrl = shortUrl.TrimStart(badChars).TrimEnd(badChars);

            log.LogInformation(string.Format("Short URL requested for {0}, seeking url for key '{1}'", req.Path, shortUrl));

            var cachedUrl = urlCache.Get(shortUrl);
            if (cachedUrl == null) {
            
                var storageCreds = new StorageCredentials(Environment.GetEnvironmentVariable("AccountName"),Environment.GetEnvironmentVariable("AccountKey"));
                var storageAccount = new CloudStorageAccount(storageCreds, useHttps: true);
                var storageClient = storageAccount.CreateCloudTableClient();
                var storageTable = storageClient.GetTableReference(Environment.GetEnvironmentVariable("TableName"));
                
                var query = new TableQuery<ShortenerRow>().Where(
                    TableQuery.CombineFilters(
                        TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, Environment.GetEnvironmentVariable("PartitionKey")),
                        TableOperators.And,
                        TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, shortUrl)
                    )
                );
                var recordSet = storageTable.ExecuteQuery(query).ToList();

                if(recordSet.Count == 0) {
                    log.LogInformation(String.Format("Table query returned no results, redirect/302ing to {0} (default)", defaultUrl));
                    redirectUrl = defaultUrl;
                } else {
                    log.LogInformation(string.Format("Table query returned {0}, storing in cache and redirect/302ing", recordSet[0].Url));
                    redirectUrl = recordSet[0].Url;
                }

                var policy = new MemoryCacheEntryOptions();
                policy.Priority = CacheItemPriority.Normal;
                policy.AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(5);
                urlCache.Set(shortUrl, redirectUrl, policy);

            } else {
                log.LogInformation(string.Format("Cache hit! Redirecting to {0}", cachedUrl));
                redirectUrl = cachedUrl.ToString();
            }

            return new RedirectResult(redirectUrl, false);
        }
    }
}
