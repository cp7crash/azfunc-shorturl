using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace SirSuperGeek.AzFunc.ShortUrl {

    interface ICmsHandler {

        string Type { get; }

        List<ShortUrlItem> ContentItems { get; }
        
        Task<IActionResult> GetContentItems();

    }
    
}