# azfunc-shorturl
A super simple Azure (v3) Function URL shortener service, which uses Azure table storage as its back-end.

## table storage setup
See [Quickstart: Create an Azure Storage table in the Azure portal](https://docs.microsoft.com/en-us/azure/storage/tables/table-storage-quickstart-portal) for basic setup.

Function currently uses a single partition, the `key` is the (case sensitive) short URL and we'll redirect to the `Url` value or the `DefaultRedirect` setting if key is not found. Leading and trailing forward `/` and backward `\` slashes are removed from keys and should not be present in the table. Entries are added using [Azure Storage Explorer](https://azure.microsoft.com/en-gb/features/storage-explorer) but might @todo provide an additional method to pull content into the table from a CMS (e.g. prismic) if desired.

## custom apex domain setup 
Short domains should be short, but as custom domains for functions depend upon CNAMEs (and the RFC doesn't permit a CNAME to reference root), getting an ultra short domain on a function is tricky. There are no doubt lots of ways to solve this problem (all including additional components), but Frontdoor isn't too tricky to set up:

1. Register a domain, add it to Azure DNS and get your registrar pointing at the provided name servers
0. Add an apex domain cert to a KeyVault (Frontdoor supports auto. cert. generation, but not for bare domains, they also have a [limited CA list](https://docs.microsoft.com/en-us/azure/frontdoor/front-door-troubleshoot-allowed-ca) to be aware of)
0. Allow the Frontdoor service access to your KeyVault (see [Tutorial: Configure HTTPS on a Front Door custom domain](https://docs.microsoft.com/en-us/azure/frontdoor/front-door-custom-domain-https?tabs=option-2-enable-https-with-your-own-certificate)
0. Create a Frontdoor instance with defaults
0. Add the custom domain to Frontdoor (see [Tutorial: Add a custom domain to your Front Door](https://docs.microsoft.com/en-us/azure/frontdoor/front-door-custom-domain))
0. Enable custom domain HTTPS and select use own certificate
0. Add your function as a back-end pool (disable health probe because this is handled elsewhere and incurs cost)
0. Add an HTTPS routing rule for your short domain /* to the Function back-end pool
0. Add a http->https redirect for your short domain
0. With a bit of luck, that's it!

## caching
The function uses a simple in-memory cache with a TTL of five minutes, aligned with the non-durable function disposition timer.

## settings
Sample `local.settings.json` for our https://ct.lol domain

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
    "DefaultRedirect": "https://cloudthing.com",
    "refreshKey": "<a secret>"
  },
  "ConnectionStrings": {
  }
}
```
On deployment, relevant configuration will need setting manually or via an ARM template.