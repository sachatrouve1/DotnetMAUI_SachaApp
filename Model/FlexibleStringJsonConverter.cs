using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SachaApp.Model;

// Allows API fields that sometimes come as numbers and sometimes as strings.
public sealed class FlexibleStringJsonConverter : JsonConverter<string?>
{
    public override string? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.TokenType switch
        {
            JsonTokenType.Null => null,
            JsonTokenType.String => reader.GetString(),
            JsonTokenType.Number => reader.TryGetInt64(out var intValue)
                ? intValue.ToString(CultureInfo.InvariantCulture)
                : reader.GetDouble().ToString(CultureInfo.InvariantCulture),
            JsonTokenType.True => bool.TrueString,
            JsonTokenType.False => bool.FalseString,
            _ => ReadRawJson(ref reader)
        };
    }

    private static string ReadRawJson(ref Utf8JsonReader reader)
    {
        using var document = JsonDocument.ParseValue(ref reader);
        return document.RootElement.GetRawText();
    }

    public override void Write(Utf8JsonWriter writer, string? value, JsonSerializerOptions options)
    {
        if (value is null)
        {
            writer.WriteNullValue();
            return;
        }

        writer.WriteStringValue(value);
    }
}

