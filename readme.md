# azfunc-shorturl
A simple url shortener service function which uses Azure table storage as its db.

## table storage setup
Currently uses a single partition, the key is the (case sensitive) shorturl and we'll redirect to the `Url` value or the `DeafultRedirect` setting if key is not found. Leading and trailing forward `/` and backward `\` slashes are removed from keys and should not be present in the table. At present entries are added using [Azure Storage Explorer](https://azure.microsoft.com/en-gb/features/storage-explorer) but might @todo provide an additional method to pull content into the table from a CMS (e.g. prismic).

## custom domain setup
Functions at the domain apex are tricky, and while there's no doubt there are plenty of other ways, it's suggested you 

## caching
Uses a simple in-memory cache with a TTL of five minutes, aligned with non-durable function disposation timer.

## settings
Sample local.settings.json

``` csharp
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet",
    "PartitionKey": "ctlol",
    "AccountName": "ctlol",
    "TableName": "ctlol",
    "AccountKey": "<a storage account key>",
    "DefaultRedirect": "https://cloudthing.com"
  },
  "ConnectionStrings": {
    
  }
}
On deployment, relevant configuration will need setting manually or via an ARM template.
```
