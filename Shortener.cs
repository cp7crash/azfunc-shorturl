using Microsoft.Azure.Cosmos.Table;

namespace SirSuperGeek.AzFunc.ShortUrl
{
    public class Shortener : TableEntity
        {
            public string Url { get; set; }
        }
}