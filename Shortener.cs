using Microsoft.WindowsAzure.Storage.Table;

namespace SirSuperGeek.AzFunc.ShortUrl
{
    public class Shortener : TableEntity
        {
            public string RedirectUrl { get; set; }
        }
}