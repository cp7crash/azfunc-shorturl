using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Net.Http;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace SirSuperGeek.AzFunc.ShortUrl.Prismic {

    public class PrismicHandler : CmsHandler {

        public readonly string repo;
        public readonly string apiRoot;
        public readonly string refType;
        public readonly string contentType;
        private string targetRef;
        private HttpClient apiClient;
        
        public PrismicHandler(string cmsType, string cmsSettings, ref ILogger log) : base(cmsType, cmsSettings, ref log) {
            
            repo = (string)Settings["repo"];
            apiRoot = $"https://{repo}.prismic.io/api/v2";
            refType = (string)Settings["ref"];
            contentType = (string)Settings["type"];
            apiClient = new HttpClient();

        }

        public async override Task<IActionResult> GetContentItems() {
           
            // fetch desired ref (e.g. master)
            _log.LogInformation($"Attempting to update content from {apiRoot}");
            
            var content = await apiClient.GetStringAsync(apiRoot);
            var rootResponse =  JsonConvert.DeserializeObject<Prismic.RootResponse>(content);
            targetRef = rootResponse.RefByType(refType);
        
            _log.LogInformation($"Sought ref of type {refType} and retrieved {targetRef}");
            if(targetRef == null) {
                _log.LogError($"Unable to refresh content from prismic as unable to locate {refType} ref");
                return new BadRequestResult();
            }
            
            string contentFilter = WebUtility.UrlEncode($"[[at(document.type, \"{contentType}\")]]");
            return await getPageOfItems(contentFilter, 1);

       }

        private async Task<IActionResult> getPageOfItems(string filter, int pageSize, int pageNumber = 1) {
            
            string contentQuery = $"documents/search?ref={targetRef}&q={filter}&pageSize={pageSize}&page={pageNumber}";
            _log.LogInformation($"Attempting to fetch content at {contentQuery}");
            var content = await apiClient.GetStringAsync($"{apiRoot}/{contentQuery}");
            
            var resultsResponse = JsonConvert.DeserializeObject<Prismic.ResultsResponse>(content);
            _log.LogInformation($"Found {resultsResponse.TotalResultsSize} {contentType} documents: at page {pageNumber} of {resultsResponse.TotalPages} with page size {pageSize}");
            
            if(resultsResponse.TotalResultsSize == 0)
                return new NoContentResult();

            foreach(Result result in resultsResponse.ResultsResults)
                ContentItems.Add(new ContentItem() {Key = result.Data.Key, Url = result.Data.Url.UrlUrl.OriginalString});

            if(resultsResponse.TotalPages > pageNumber)
                return await getPageOfItems(filter, pageSize, pageNumber + 1);

            return new OkResult();

       }

    }

}