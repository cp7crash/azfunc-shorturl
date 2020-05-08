using Microsoft.Azure.Cosmos.Table;
using System.Linq;
using System.Collections.Generic;

namespace SirSuperGeek.AzFunc.ShortUrl {

    public class TableHandler {

        public CloudTable TableClient;
        public readonly string PartitionKey;

        
        public TableHandler(string storageAccount, string accountKey, string tableName, string partitionKey) {

            var creds = new StorageCredentials(storageAccount, accountKey);
            var account = new CloudStorageAccount(creds, useHttps: true);
            var client = account.CreateCloudTableClient();
            TableClient = client.GetTableReference(tableName);
            PartitionKey = partitionKey;

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

        public void Store (List<ContentItem> contentItems) {

            var batch = new TableBatchOperation();
            

        }
    }


}