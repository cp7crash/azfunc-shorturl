using Microsoft.Azure.Cosmos.Table;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace cp7crash.AzFunc.ShortUrl {

    public class TableHandler {

        public CloudTable TableClient;
        public readonly string PartitionKey;
        protected ILogger _log;
        private List<ShortUrlItem> urlItems;
        private int batchSize = 100;

        public TableHandler(string storageAccount, string accountKey, string tableName, string partitionKey, ILogger log) {

            var creds = new StorageCredentials(storageAccount, accountKey);
            var account = new CloudStorageAccount(creds, useHttps: true);
            var client = account.CreateCloudTableClient();
            TableClient = client.GetTableReference(tableName);
            PartitionKey = partitionKey;
            _log = log;

        }

        public string Seek(string key) {
            
            var query = new TableQuery<ShortUrlItem>().Where(
                TableQuery.CombineFilters(
                    TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, PartitionKey),
                    TableOperators.And,
                    TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, key)
                )
            );
            var recordSet = TableClient.ExecuteQuery(query).ToList();

            return (recordSet.Count() == 0) ? null : recordSet[0].Url;

        }

        public void Store (List<ShortUrlItem> contentItems) {

            urlItems = contentItems;
            storeBatch(0);

        }

        public void storeBatch(int startPointer) {

            int endPointer = startPointer + batchSize;
            if(endPointer > urlItems.Count - 1) 
                endPointer = urlItems.Count;
            var batch = new TableBatchOperation();

            for(int i = startPointer; i < endPointer; i++) {
                urlItems[i].PartitionKey = PartitionKey;
                batch.InsertOrReplace(urlItems[i]);
            }
            
            _log.LogInformation($"Batch inserting {batch.Count} items, {startPointer} to {endPointer} of {urlItems.Count} total");
            TableClient.ExecuteBatch(batch);

            if(endPointer < urlItems.Count)
                storeBatch(endPointer);


        }
    }


}