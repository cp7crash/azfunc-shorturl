# azfunc-shorturl
A super simple Azure (v3) Function URL shortener service, which uses Azure table storage as its back-end.

Entries can be added using [Azure Storage Explorer](https://azure.microsoft.com/en-gb/features/storage-explorer) or automatically using a CMS handler (prismic.io currently supported).

## table storage setup
See [Quickstart: Create an Azure Storage table in the Azure portal](https://docs.microsoft.com/en-us/azure/storage/tables/table-storage-quickstart-portal) for basic setup.

Function currently uses a single partition, the `key` is the (case sensitive) short URL and we'll redirect to the `Url` value or the `DefaultRedirect` setting if key is not found. Leading and trailing forward `/` and backward `\` slashes are removed from keys and should not be present in the table. 

## custom apex domain setup 
Short domains should be short, but as custom domains for functions depend upon CNAMEs (and the RFC doesn't permit a CNAME to reference root), getting an ultra short domain on a function is tricky. There are no doubt lots of ways to solve this problem (all including additional components), but Frontdoor isn't too tricky to set up:

1. Register a domain, add it to Azure DNS and get your registrar pointing at the provided name servers
0. Add an apex domain cert to a KeyVault (Frontdoor supports auto. cert. generation, but not for bare domains, they also have a [limited CA list](https://docs.microsoft.com/en-us/azure/frontdoor/front-door-troubleshoot-allowed-ca) to be aware of)
0. Allow the Frontdoor service access to your KeyVault (see [Tutorial: Configure HTTPS on a Front Door custom domain](https://docs.microsoft.com/en-us/azure/frontdoor/front-door-custom-domain-https?tabs=option-2-enable-https-with-your-own-certificate)
0. Create a Frontdoor instance with defaults
0. Add the custom domain to Frontdoor (see [Tutorial: Add a custom domain to your Front Door](https://docs.microsoft.com/en-us/azure/frontdoor/front-door-custom-domain))
0. Enable custom domain HTTPS and select use own certificate, select the certificate added to KeyVault earlier
0. Add your function as a back-end pool: disable the health probe if needed (handled elsewhere and incurs cost), leave alone if you want your function to be kept alive
0. Add an HTTPS only routing rule for your short domain /* to the Function back-end pool
0. Add a http->https redirect for your short domain
0. With a bit of luck, that's it!

## cms refresh setup
The function can be set up to pull in documents and build the storage table through a back-end CMS, assuming the appropriate cms settings are provided. The desired CMS handler can be specified through the `CmsType` configuration entry, it's associated settings within a comma delimited list within `CmsSettings` and a private/special URL within `CmsRefreshKey`. CMS handlers can be built through the `ICmsHandler` interface and `CmsHandler` base class and the following pre-built handlers are included:

0. [Prismic.io](PrismicHandler/prismic-setup.md)

## caching
The function uses a simple in-memory cache with a TTL of five minutes, aligned with the non-durable function disposition timer.

## sample settings
Sample `local.settings.json` for our https://ct.lol domain

``` csharp
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "",
    "AzureWebJobsDisableHomepage": true,
    "FUNCTIONS_WORKER_RUNTIME": "dotnet",
    "ShortUrl.DefaultRedirect": "https://cloudthing.com",
    "ShortUrl.CmsType": "Prismic",
    "ShortUrl.CmsSettings": "repo=launchparty,ref=master,type=ct-short-url",
    "ShortUrl.CmsRefreshKey": "<some secret>",
    "ShortUrl.StorageAccount": "ctlol",
    "ShortUrl.StorageKey": "<a storage account key>",
    "ShortUrl.TableName": "ctlol",
    "ShortUrl.PartitionKey": "ctlol"
  },
  "ConnectionStrings": {
  }
}
```
On deployment, relevant configuration will need setting manually within azure console or via an ARM template.