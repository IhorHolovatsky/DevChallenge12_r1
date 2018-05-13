using System;

namespace CDN.Domain.Models
{
    public class CdnFileObject
    {
        public string Id { get; set; }
        public int VersionId { get; set; }
        public int ServerId { get; set; }
        public long Size { get; set; }
        public DateTime? DateUploaded { get; set; }
        public DateTime? LastAccess { get; set; }
        public Guid? UploadId { get; set; }
    }
}