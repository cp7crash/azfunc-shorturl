using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Cosmos.Table;

namespace SirSuperGeek.AzFunc.ShortUrl {
    
    using Prismic;
    public static class ShortUrl {
        
        static MemoryCache urlCache = new MemoryCache(new MemoryCacheOptions());
        static ILogger log;

        [FunctionName("ShortUrl")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "{*all}")] HttpRequest req, ILogger logger) {
            
            log = logger;

            var badChars = "/\\".ToCharArray();
            string shortUrl = req.Path;
            shortUrl = shortUrl.TrimStart(badChars).TrimEnd(badChars);

            if(string.Equals(shortUrl, config("CmsRefreshKey"))) {
                await Task.Run(() => {
                    return refreshFromCms();
                });
            }
            
            string redirectUrl;
            log.LogInformation($"Short URL requested for {req.Path}, seeking key '{shortUrl}'");

            var cachedUrl = urlCache.Get(shortUrl);
            if (cachedUrl == null) {

                string defaultUrl = config("DefaultRedirect");
                var targetUrl = seekInTable(shortUrl);
                if(targetUrl == null) {
                    log.LogInformation($"Table query returned no result, redirect/302ing to {defaultUrl} (default)");
                    redirectUrl = defaultUrl;
                } else {
                    log.LogInformation($"Table query returned {targetUrl}, redirect/302ing");
                    redirectUrl = targetUrl;
                }

                // add key to cache regardless to minimise requests for undefined keys
                var policy = new MemoryCacheEntryOptions();
                policy.Priority = CacheItemPriority.Normal;
                policy.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
                urlCache.Set(shortUrl, redirectUrl, policy);

            } else {
                log.LogInformation($"Cache hit! Redirecting to {cachedUrl}");
                redirectUrl = cachedUrl.ToString();
            }

            return new RedirectResult(redirectUrl, false);
        }

        private static string seekInTable(string key) {

            var storageTable = table;
            
            var query = new TableQuery<TableRow>().Where(
                TableQuery.CombineFilters(
                    TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, config("PartitionKey")),
                    TableOperators.And,
                    TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, key)
                )
            );
            var recordSet = storageTable.ExecuteQuery(query).ToList();

            return (recordSet.Count() == 0) ? null : recordSet[0].Url;

        }

        private static CloudTable table { get {

            var storageCreds = new StorageCredentials(config("StorageAccount"), config("StorageKey"));
            var storageAccount = new CloudStorageAccount(storageCreds, useHttps: true);
            var storageClient = storageAccount.CreateCloudTableClient();
            return storageClient.GetTableReference(config("TableName"));

        }}

        private static async Task<IActionResult> refreshFromCms() {
        
            var cmsHandler = new PrismicHandler(config("CmsType"), config("CmsSettings"), ref log);
            await cmsHandler.GetContentItems();

            //var cmsHandlerType = Type.GetType(string.Format("{0}Handler", config("CmsType")));
            //CmsHandler cmsHandler = Activator.CreateInstance<CmsHandler>(cmsHandlerType);

            
            return new OkResult();
            
        }

        private static string config(string configItemName) {

            var configValue = Environment.GetEnvironmentVariable($"ShortUrl.{configItemName}");
            if(String.IsNullOrEmpty(configValue))
                log.LogError($"Unable to find a value for {configItemName}");
            
            return configValue;

        }
    }
}
