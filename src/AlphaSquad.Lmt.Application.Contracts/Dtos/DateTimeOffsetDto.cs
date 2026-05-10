using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AlphaSquad.Lmt.Application.Contracts.Dtos;

/// <summary>
/// Representa um DateTimeOffset serializado pela API.
/// A camada LMT usa este wrapper para conseguir mapear datas do contrato sem perder compatibilidade com o Kiota.
/// </summary>
[JsonConverter(typeof(DateTimeOffsetDtoJsonConverter))]
public sealed class DateTimeOffsetDto
{
    public DateTimeOffsetDto()
    {
    }

    public DateTimeOffsetDto(DateTimeOffset value)
    {
        Value = value;
    }

    public DateTimeOffset Value { get; set; }

    public DateTimeOffset ToDateTimeOffset() => Value;

    public override string ToString() => Value.ToString("O", CultureInfo.InvariantCulture);

    public static implicit operator DateTimeOffset(DateTimeOffsetDto value) => value.Value;

    public static implicit operator DateTimeOffsetDto(DateTimeOffset value) => new(value);
}

/// <summary>
/// Permite que o contrato leia e escreva datas ISO-8601 diretamente, mesmo usando um DTO intermediario.
/// </summary>
public sealed class DateTimeOffsetDtoJsonConverter : JsonConverter<DateTimeOffsetDto>
{
    public override DateTimeOffsetDto? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
            return null;

        if (reader.TokenType == JsonTokenType.String)
        {
            var raw = reader.GetString();

            if (string.IsNullOrWhiteSpace(raw))
                return null;

            if (DateTimeOffset.TryParse(raw, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var parsed))
                return new DateTimeOffsetDto(parsed);
        }

        if (reader.TokenType == JsonTokenType.StartObject)
        {
            using var document = JsonDocument.ParseValue(ref reader);
            var root = document.RootElement;

            if (root.TryGetProperty("value", out var valueElement) &&
                valueElement.ValueKind == JsonValueKind.String &&
                DateTimeOffset.TryParse(valueElement.GetString(), CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var objectParsed))
            {
                return new DateTimeOffsetDto(objectParsed);
            }
        }

        throw new JsonException("The JSON value could not be converted to DateTimeOffsetDto.");
    }

    public override void Write(Utf8JsonWriter writer, DateTimeOffsetDto value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.Value);
    }
}
