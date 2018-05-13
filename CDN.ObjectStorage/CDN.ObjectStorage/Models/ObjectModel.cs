using System;
using System.Collections.Generic;

namespace CDN.OriginServer.Api.Models
{
    public class ObjectModel
    {
        public string UploadId { get; set; }
        public string ObjectKey { get; set; }
        public string Version { get; set; }
        public List<ObjectChunkModel> Parts { get; set; } = new List<ObjectChunkModel>();

        public bool IsCompleted { get; set; }
        public bool IsProcessing { get; set; }

        public long? ObjectSize { get; set; }

        public DateTime? UploadDateTime { get; set; }
        public DateTime? LastAccessDateTime { get; set; }
    }
}