# azfunc-shorturl
A super simple Azure (v3) Function URL shortener service, which uses Azure table storage as its back-end.

## table storage setup
See [Quickstart: Create an Azure Storage table in the Azure portal](https://docs.microsoft.com/en-us/azure/storage/tables/table-storage-quickstart-portal) for basic setup.

Function currently uses a single partition, the key is the (case sensitive) short URL and we'll redirect to the `Url` value or the `DefaultRedirect` setting if key is not found. Leading and trailing forward `/` and backward `\` slashes are removed from keys and should not be present in the table. Entries are added using [Azure Storage Explorer](https://azure.microsoft.com/en-gb/features/storage-explorer) but might @todo provide an additional method to pull content into the table from a CMS (e.g. prismic), however flow might be better here.

## custom domain setup
Short domains should be short, but due to the CNAME dependency for custom domains, publishing functions at the apex is tricky. One route to solving this is using Frontdoor:

1. Register a domain
0. Create new zone in Azure DNS
0. Set name servers at registrar to Azure DNS provided name servers
0. Add your apex domain cert (Frontdoor only supports 1 level wildcards) to a KeyVault instance
0. Create a Frontdoor instance with defaults
0. Add an A alias record in Azure DNS and select the Frontdoor instance
0. Add the custom domain to Frontdoor designer
0. Enable custom domain HTTPS and select use own certificate
0. Follow PS instructions to grant app access to your KeyVault instance for your cert
0. Add your function as a back-end pool (disable health probe because this is handled elsewhere and incurs cost)
0. Add a routing rule for your short domain /* to the Function back-end pool

## caching
Uses a simple in-memory cache with a TTL of five minutes, aligned with the non-durable function disposition timer.

## settings
Sample local.settings.json for our https://ct.lol domain

``` csharp
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "",
    "AzureWebJobsDisableHomepage": true,
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
```
On deployment, relevant configuration will need setting manually or via an ARM template.