using CDN.Domain.Models;

namespace CDN.Domain.Configuration
{
    public class CdnOptions
    {
        public string DatabaseConnection { get; set; }
        public CdnServer Server { get; set; }
    }
}
