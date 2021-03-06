using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;


namespace cp7crash.AzFunc.ShortUrl {
    
    using Prismic;

    public static class ShortUrl {
        
        static MemoryCache urlCache = new MemoryCache(new MemoryCacheOptions());
        static ILogger log;


        [FunctionName("ShortUrl")]
        public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "{*all}")] HttpRequest req, ILogger logger) {
            
            log = logger;

            var badChars = "/\\".ToCharArray();
            string shortUrl = req.Path;
            shortUrl = shortUrl.TrimStart(badChars).TrimEnd(badChars);

            if(string.Equals(shortUrl, config("CmsRefreshKey"))) {
                return await Task.Run(() => {
                    return refreshFromCms();
                });
            }
            
            string redirectUrl;
            log.LogInformation($"Short URL requested for {req.Path}, seeking key '{shortUrl}'");

            var cachedUrl = urlCache.Get(shortUrl);
            if (cachedUrl == null) {

                var tableClient = getTableClient();
                string defaultUrl = config("DefaultRedirect");
                var targetUrl = tableClient.Seek(shortUrl);
                if(string.IsNullOrEmpty(targetUrl)) {
                    log.LogInformation($"Table query returned no or empty result, redirect/302ing to {defaultUrl} (default)");
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

        private static async Task<IActionResult> refreshFromCms() {
        
            var cmsType = config("CmsType");
            if(string.IsNullOrEmpty(cmsType)) {
                log.LogError($"Unable to refresh content from CMS as CmsType not set");
                return new NoContentResult();
            }
            var handlerType = Type.GetType(string.Format("SirSuperGeek.AzFunc.ShortUrl.{0}.{0}Handler", cmsType));
            
            log.LogInformation($"Attempting to activate an instance of {handlerType}");
            var cmsHandler = (CmsHandler)Activator.CreateInstance(handlerType, config("CmsType"), config("CmsSettings"), log);
            var resultObj = await cmsHandler.GetContentItems();

            if(resultObj.GetType() != typeof(OkResult)) {
                log.LogError($"Unable to refresh content from CMS, call returned {resultObj.GetType()}");
                return resultObj;
            }

            if(cmsHandler.ContentItems.Count() < 1) {
                log.LogWarning($"Unable to refresh content from CMS, {cmsHandler.ContentItems.Count()} items discovered.");
                return new NoContentResult();
            }

            var tableClient = getTableClient();
            tableClient.Store(cmsHandler.ContentItems);
                            
            return new OkResult();
            
        }

        private static TableHandler getTableClient() {

            return new TableHandler(config("StorageAccount"), config("StorageKey"), config("TableName"), config("PartitionKey"), log);
        }

        private static string config(string configItemName) {

            var configValue = Environment.GetEnvironmentVariable($"ShortUrl.{configItemName}");
            if(String.IsNullOrEmpty(configValue))
                log.LogWarning($"Unable to find a value for {configItemName}");
            
            return configValue;

        }
    }
}
