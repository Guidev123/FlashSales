using System.Text.Json;

namespace FlashSales.Application.Extensions
{
    public static class JsonSerializerOptionsExtensions
    {
        private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions()
        {
            PropertyNameCaseInsensitive = true
        };

        public static JsonSerializerOptions GetDefault()
        {
            return _jsonOptions;
        }
    }
}