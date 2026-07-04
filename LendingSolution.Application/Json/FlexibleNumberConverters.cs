using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace LendingSolution.Application.Json;

/// <summary>
/// JSON converters that tolerate numeric values arriving as JSON strings
/// (e.g. "7,659.00", "5000", "") — common with Nigerian credit bureau APIs
/// surfaced through Mono. Empty/invalid strings map to the type's default.
/// </summary>
public static class FlexibleNumber
{
    private static string? ReadRaw(ref Utf8JsonReader reader) => reader.TokenType switch
    {
        JsonTokenType.String => reader.GetString(),
        JsonTokenType.Number => reader.GetDecimal().ToString(CultureInfo.InvariantCulture),
        JsonTokenType.Null => null,
        _ => throw new JsonException($"Unexpected token {reader.TokenType} for numeric value.")
    };

    private static string? Clean(string? s)
    {
        if (string.IsNullOrWhiteSpace(s)) return null;
        // Strip thousands separators / currency symbols / stray spaces.
        return s.Replace(",", string.Empty).Replace("₦", string.Empty).Trim();
    }

    public sealed class DecimalConverter : JsonConverter<decimal>
    {
        public override decimal Read(ref Utf8JsonReader reader, Type t, JsonSerializerOptions o)
        {
            var raw = Clean(ReadRaw(ref reader));
            return decimal.TryParse(raw, NumberStyles.Any, CultureInfo.InvariantCulture, out var v) ? v : 0m;
        }
        public override void Write(Utf8JsonWriter writer, decimal value, JsonSerializerOptions o)
            => writer.WriteNumberValue(value);
    }

    public sealed class NullableDecimalConverter : JsonConverter<decimal?>
    {
        public override decimal? Read(ref Utf8JsonReader reader, Type t, JsonSerializerOptions o)
        {
            var raw = Clean(ReadRaw(ref reader));
            return decimal.TryParse(raw, NumberStyles.Any, CultureInfo.InvariantCulture, out var v) ? v : null;
        }
        public override void Write(Utf8JsonWriter writer, decimal? value, JsonSerializerOptions o)
        {
            if (value.HasValue) writer.WriteNumberValue(value.Value); else writer.WriteNullValue();
        }
    }

    public sealed class IntConverter : JsonConverter<int>
    {
        public override int Read(ref Utf8JsonReader reader, Type t, JsonSerializerOptions o)
        {
            var raw = Clean(ReadRaw(ref reader));
            return decimal.TryParse(raw, NumberStyles.Any, CultureInfo.InvariantCulture, out var v)
                ? (int)Math.Truncate(v) : 0;
        }
        public override void Write(Utf8JsonWriter writer, int value, JsonSerializerOptions o)
            => writer.WriteNumberValue(value);
    }

    public sealed class NullableIntConverter : JsonConverter<int?>
    {
        public override int? Read(ref Utf8JsonReader reader, Type t, JsonSerializerOptions o)
        {
            var raw = Clean(ReadRaw(ref reader));
            return decimal.TryParse(raw, NumberStyles.Any, CultureInfo.InvariantCulture, out var v)
                ? (int)Math.Truncate(v) : null;
        }
        public override void Write(Utf8JsonWriter writer, int? value, JsonSerializerOptions o)
        {
            if (value.HasValue) writer.WriteNumberValue(value.Value); else writer.WriteNullValue();
        }
    }
}
