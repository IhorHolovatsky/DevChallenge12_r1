namespace CDN.Domain.Configuration
{
    public class StorageOptions
    {
        public string BaseFolder { get; set; }
        public int CleanUpCheckInterval { get; set; }
        public int CleanUpWhenAvailMemoryLessThanPercents { get; set; }
        public int CleanUpObjectAgeInDays { get; set; }
    }
}