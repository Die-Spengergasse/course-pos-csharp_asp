using System;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace IdHasher;

public class IdConverter : JsonConverter<Id>
{
    public override Id Read(
        ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.HasValueSequence)
            return Id.Decode(reader.GetString() ?? string.Empty);

        Span<char> chars = stackalloc char[reader.ValueSpan.Length];
        int written = Encoding.UTF8.GetChars(reader.ValueSpan, chars);
        return Id.Decode(chars[..written]);
    }

    public override void Write(Utf8JsonWriter writer, Id id, JsonSerializerOptions options) =>
        writer.WriteStringValue(id.EncodedValue);
}
