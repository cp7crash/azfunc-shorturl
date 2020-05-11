using Microsoft.Azure.Cosmos.Table;

namespace cp7crash.AzFunc.ShortUrl
{
    public class ShortUrlItem : TableEntity
        {
            public string Url { get; set; }
        }
}