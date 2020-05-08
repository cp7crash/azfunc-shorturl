using Microsoft.Azure.Cosmos.Table;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace SirSuperGeek.AzFunc.ShortUrl {

    public class TableHandler {

        public CloudTable TableClient;
        public readonly string PartitionKey;
        protected ILogger _log;

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

            var batch = new TableBatchOperation();
            foreach(ShortUrlItem item in contentItems) {
                item.PartitionKey = PartitionKey;
                batch.InsertOrReplace(item);
            }
            _log.LogInformation($"Attempting batch insert of {batch.Count}");
            
            TableClient.ExecuteBatch(batch);

        }
    }


}