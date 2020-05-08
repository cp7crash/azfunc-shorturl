using Microsoft.Azure.Cosmos.Table;

namespace SirSuperGeek.AzFunc.ShortUrl
{
    public class ShortUrlItem : TableEntity
        {
            public string Url { get; set; }
        }
}