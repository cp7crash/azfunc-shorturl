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


namespace SirSuperGeek.AzFunc.ShortUrl {
    
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
                await Task.Run(() => {
                    return refreshFromCms();
                });
            }
            
            string redirectUrl;
            log.LogInformation($"Short URL requested for {req.Path}, seeking key '{shortUrl}'");

            var cachedUrl = urlCache.Get(shortUrl);
            if (cachedUrl == null) {

                var tableClient = new TableHandler(config("StorageAccount"), config("StorageKey"), config("TableName"), config("PartitionKey"));
                string defaultUrl = config("DefaultRedirect");
                var targetUrl = tableClient.Seek(shortUrl);
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

        

        private static async Task<IActionResult> refreshFromCms() {
        
            if(string.IsNullOrEmpty(config("CmsType"))) {
                log.LogError($"Unable to refresh content from CMS as CmsType not set");
                return new NoContentResult();
            }

            var cmsHandler = new PrismicHandler(config("CmsType"), config("CmsSettings"), ref log);
            var resultObj = await cmsHandler.GetContentItems();

            if(resultObj.GetType() != typeof(OkResult)) {
                log.LogError($"Unable to refresh content from CMS, call returned {resultObj.GetType()}");
                return resultObj;
            }

            if(cmsHandler.ContentItems.Count() < 1) {
                log.LogWarning($"Unable to refresh content from CMS, {cmsHandler.ContentItems.Count()} items discovered.");
                return new NoContentResult();
            }
                

            //var cmsHandlerType = Type.GetType(string.Format("{0}Handler", config("CmsType")));
            //CmsHandler cmsHandler = Activator.CreateInstance<CmsHandler>(cmsHandlerType);

            
            return new OkResult();
            
        }

        private static string config(string configItemName) {

            var configValue = Environment.GetEnvironmentVariable($"ShortUrl.{configItemName}");
            if(String.IsNullOrEmpty(configValue))
                log.LogWarning($"Unable to find a value for {configItemName}");
            
            return configValue;

        }
    }
}
