using Microsoft.Azure.Cosmos.Table;

namespace SirSuperGeek.AzFunc.ShortUrl
{
    public class TableRow : TableEntity
        {
            public string Url { get; set; }
        }
}