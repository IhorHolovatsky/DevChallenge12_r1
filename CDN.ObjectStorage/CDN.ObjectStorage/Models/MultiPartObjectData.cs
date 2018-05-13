using System.IO;

namespace CDN.OriginServer.Api.Models
{
    public class MultiPartObjectData
    {
        public Stream Stream { get; set; }
        public int Start { get; set; }
        public long End { get; set; }
        public long FileLength { get; set; }
    }
}