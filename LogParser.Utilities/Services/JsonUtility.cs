using System.Text.Encodings.Web;
using System.Text.Json;

namespace LogParser.Utilities.Services
{
    public static class JsonUtility
    {
        private static readonly JsonSerializerOptions _options = new JsonSerializerOptions
        {
            WriteIndented = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        public static JsonSerializerOptions Options => _options;

        public static string Serialize(object value)
        {
            return JsonSerializer.Serialize(value, _options);
        }

        public static T Deserialize<T>(string json)
        {
            return JsonSerializer.Deserialize<T>(json, _options)
                   ?? throw new InvalidOperationException("Deserialization returned null.");
        }

        public static object? Deserialize(string json, Type returnType)
        {
            return JsonSerializer.Deserialize(json, returnType, _options);
        }
    }
}
