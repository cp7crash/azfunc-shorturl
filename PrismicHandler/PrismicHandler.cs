using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Http;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace SirSuperGeek.AzFunc.ShortUrl.Prismic {

    public class PrismicHandler : CmsHandler {

        private string repo;
        private string refType;
        private string contentType;
        private string apiRoot;
        private string targetRef;

        public PrismicHandler(string cmsType, string cmsSettings, ref ILogger log) : base(cmsType, cmsSettings, ref log) {
            
            repo = (string)Settings["repo"];
            refType = (string)Settings["ref"];
            contentType = (string)Settings["type"];
            apiRoot = $"https://{repo}.prismic.io/api/v2";

        }

        public async override Task<IActionResult> GetContentItems() {
           
            // fetch desired ref (e.g. master)
            using (var apiClient = new HttpClient()) {
                var content = await apiClient.GetStringAsync(apiRoot);
                var rootResponse =  JsonConvert.DeserializeObject<Prismic.RootResponse>(content);
                targetRef = rootResponse.RefByType(refType);
            }
            _log.LogInformation($"Sought ref of type {refType} and retrieved {targetRef}");
            if(targetRef == null) {
                _log.LogError($"Unable to refresh content from prismic as unable to locate {refType} ref");
                return new BadRequestResult();
            }
            
            // fetch items of specified type [at(document.type, "ct-short-url")]
            // @todo: support more than 100 items pageSize=1, page=N
            string typeFilter = WebUtility.UrlEncode($"[[at(document.type, \"{contentType}\")]]");
            string typeQuery = $"{apiRoot}/documents/search?ref={targetRef}&q={typeFilter}&pageSize=100";
            using (var apiClient = new HttpClient()) {
                
                var content = await apiClient.GetStringAsync(typeQuery);
                var resultsResponse = JsonConvert.DeserializeObject<Prismic.ResultsResponse>(content);
                _log.LogInformation($"Found {resultsResponse.TotalResultsSize} {contentType} documents");
                
                if(resultsResponse.TotalResultsSize == 0)
                    return new OkResult();

                foreach(Result result in resultsResponse.ResultsResults)
                    ContentItems.Add(new ContentItem() {Key = result.Data.Key, Url = result.Data.Url.UrlUrl.OriginalString});
                
                //if(resultsResponse.)
            }
            
                        
            
            return new OkResult();

       }

    }

}