using IdHasher;
using Microsoft.Extensions.Time.Testing;
using System;
using System.Buffers.Text;

namespace Eventmanager.Test;

public class IdEncoderTests
{
    [Fact]
    public void EncodeIdTest()
    {
        var secret = "cXVpY2tyb2xsbG9vc2VkaXJ0bnV0c3dpbGxveHlnZW5mb2xrc2hpbGxzaGVsbHNkb3U=";
        Id.Secret = Base64Url.DecodeFromChars(secret);
        for (int i = 0; i < 1_000_000; i += 7)
        {
            var id = new Id(i);
            var encoded = id.EncodedValue;
            var len = encoded.Length;
            var decodedId = Id.Decode(encoded);
            Assert.True(len == 48);
            Assert.True(id.Value == decodedId.Value);
        }
    }

    [Fact]
    public void FakeTimeTest()
    {
        var timeProvider = new FakeTimeProvider(
            new DateTimeOffset(2026, 2, 28, 14, 30, 0, TimeSpan.Zero));
        Assert.True(timeProvider.GetUtcNow().DateTime == new DateTime(2026, 2, 28, 14, 30, 0, DateTimeKind.Utc));
    }
}
