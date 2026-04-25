namespace FlashSales.Application.Storage
{
    public sealed class BlobStorageOptions
    {
        public const string SectionName = "BlobStorage";

        public string ConnectionString { get; set; } = null!;
        public string ContainerName { get; set; } = null!;
    }
}