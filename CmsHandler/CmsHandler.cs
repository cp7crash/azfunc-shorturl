using System.Collections;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace SirSuperGeek.AzFunc.ShortUrl {
    public abstract class CmsHandler : ICmsHandler {
        
        private string _type;
        public string Type { get => _type; }
        public Hashtable Settings { get; }
        protected ILogger _log;
        public List<ShortUrlItem> ContentItems { get; }

        public CmsHandler(string cmsType, string cmsSettings, ref ILogger log) {
            
            _type = cmsType;
            Settings = new Hashtable();
            _log = log;
            ContentItems = new List<ShortUrlItem>();

            string[] statements = cmsSettings.Split(',');
            foreach(string statement in statements) {
                string[] setting = statement.Split('=');
                Settings.Add(setting[0].Trim(), setting[1].Trim());
            }

        }
        
        public abstract Task<IActionResult> GetContentItems();
    }
}