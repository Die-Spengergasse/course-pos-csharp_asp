using System;
using System.Buffers.Binary;
using System.Buffers.Text;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Eventmanager.Application.Services;

public readonly struct Id : IParsable<Id>, IEquatable<Id>
{
    public static byte[] Secret;
    private const int _strLen = 48;  // 44 Bytes Hash (SHA256 with Base64) + 4 Bytes ID
    public static Id Empty => new Id(0);
    /// <summary>
    /// Assign a random secret. Only used if no secret is set.
    /// Important: Set a fixed secred with id.Secret to produce same results for same keys after a restart.
    /// </summary>
    static Id()
    {
        Secret = new byte[32];
        RandomNumberGenerator.Fill(Secret);
    }
    public int Value { get; }
    public Id(int value)
    {
        Value = value;
    }
    public bool HasValue => Value != 0;
    /// <summary>
    /// Encodes the value with HMACSHA256.
    /// The encoder writes the original integer value to the first four bytes of the array.
    /// The hash value is written after these four bytes.
    /// Array: V V V V H H H H ... H H (V: value, H: hash)    
    /// </summary>
    public string EncodedValue
    {
        get
        {
            return string.Create(_strLen, Value, (chars, val) =>
            {

                Span<byte> buffer = stackalloc byte[36]; // 4 (int) + 32 (hash)
                BinaryPrimitives.WriteInt32LittleEndian(buffer, val);
                HMACSHA256.HashData(Secret, buffer[..4], buffer[4..]);
                Base64Url.EncodeToChars(buffer, chars);
            });
        }
    }
    /// <summary>
    /// Check the hash value and return an instance of Id with the decoded value.
    /// If decoding fails, the value of the instance is 0.
    /// </summary>
    public static Id Decode(ReadOnlySpan<char> encodedValue)
    {
        if (encodedValue.Length != _strLen) return default;

        Span<byte> buffer = stackalloc byte[36];
        if (Base64Url.DecodeFromChars(encodedValue, buffer) != 36) return default;

        Span<byte> computedHash = stackalloc byte[32];
        HMACSHA256.HashData(Secret, buffer[..4], computedHash);

        if (!CryptographicOperations.FixedTimeEquals(buffer[4..], computedHash))
            return default;

        return new Id(BinaryPrimitives.ReadInt32LittleEndian(buffer));
    }
    public static bool operator ==(Id left, Id right) => left.Value == right.Value;
    public static bool operator !=(Id left, Id right) => left.Value != right.Value;
    public static implicit operator int(Id id) => id.Value;
    public static explicit operator Id(int value) => new Id(value);
    public override string ToString() => EncodedValue;
    public override bool Equals(object? obj) => obj is Id && Equals((Id)obj);
    public override int GetHashCode() => Value.GetHashCode();
    public bool Equals(Id other) => Value == other.Value;
    /// <summary>
    /// Converts an encoded string to an instance of Id.
    /// If the Id cannot be decoded, the value of the result is 0.
    /// The model binder requires parsing methods if you use the Id type together 
    /// with [FromRoute] or [FromQuery] in your controller.
    /// </summary>
    public static Id Parse(string s, IFormatProvider? provider) => Decode(s);
    /// <summary>
    /// Converts an encoded string to an instance of Id.
    /// If the Id cannot be decoded, the value of the result is 0.
    /// Returns false if the string cannot be a hashed id value.
    /// The model binder requires parsing methods if you use the Id type together 
    /// with [FromRoute] or [FromQuery] in your controller.
    /// </summary>
    public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out Id result)
    {
        result = Decode(s);
        return result.HasValue;
    }
}

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
