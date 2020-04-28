using Microsoft.Azure.Cosmos.Table;

namespace SirSuperGeek.AzFunc.ShortUrl
{
    public class ShortenerRow : TableEntity
        {
            public string Url { get; set; }
        }
}