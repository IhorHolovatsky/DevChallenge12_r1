namespace CDN.Domain.Models
{
    public class CdnServer
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public long FreeSpace { get; set; }
        public string IpAddress{ get; set; }
        public string Host { get; set; }
        public CdnServerRole ServerRole { get; set; }
        public bool IsOnline { get; set; }
    }
}