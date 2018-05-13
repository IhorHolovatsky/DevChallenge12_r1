using System;
using System.IO;

namespace CDN.CacheServer.Api.Models
{
    public class CachedObjectResult
    {
        public Exception Exception { get; set; }
        public bool IsSuccess { get; set; }
        public Stream FileStream { get; set; }
    }
}