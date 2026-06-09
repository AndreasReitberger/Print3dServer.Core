using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace AndreasReitberger.API.Print3dServer.Core.TypeConverters
{

    public class RegexConverter : JsonConverter<Regex>
    {
        public override Regex? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            string? pattern = reader.GetString();
            return pattern is not null ? new Regex(pattern) : null;
        }

        public override void Write(Utf8JsonWriter writer, Regex? value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value?.ToString());
        }
    }

}
