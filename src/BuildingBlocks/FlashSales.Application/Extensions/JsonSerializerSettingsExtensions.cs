using Newtonsoft.Json;

namespace FlashSales.Application.Extensions
{
    public static class JsonSerializerSettingsExtensions
    {
        public static readonly JsonSerializerSettings Instance = new()
        {
            TypeNameHandling = TypeNameHandling.All,
            MetadataPropertyHandling = MetadataPropertyHandling.ReadAhead
        };
    }
}