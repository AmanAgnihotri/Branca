// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright © 2026 Aman Agnihotri

namespace Branca.Tests;

using Xunit;

public sealed class Base62Tests
{
  [Fact(DisplayName = "Encode empty bytes as an empty string.")]
  public void EncodeEmptyBytesAsEmptyString()
  {
    Assert.Equal(string.Empty, Base62.Encode([]));
  }

  [Fact(DisplayName = "Decode an empty string as empty bytes.")]
  public void DecodeEmptyStringAsEmptyBytes()
  {
    Assert.True(Base62.TryDecode(string.Empty, out byte[] bytes));
    Assert.Empty(bytes);
  }

  [Fact(DisplayName = "Fail to decode null.")]
  public void FailToDecodeNull()
  {
    Assert.False(Base62.TryDecode(null, out _));
  }

  [Theory(DisplayName = "Fail to decode strings with invalid characters.")]
  [InlineData("_")]
  [InlineData("12!4")]
  [InlineData("1 2")]
  [InlineData("café")]
  [InlineData("♥")]
  public void FailToDecodeStringsWithInvalidCharacters(string data)
  {
    Assert.False(Base62.TryDecode(data, out _));
  }

  [Theory(DisplayName = "Fail to decode strings with leading zeros.")]
  [InlineData("00")]
  [InlineData("01")]
  [InlineData("0Zz")]
  public void FailToDecodeStringsWithLeadingZeros(string data)
  {
    Assert.False(Base62.TryDecode(data, out _));
  }

  [Fact(DisplayName = "Decode a lone zero as a single zero byte.")]
  public void DecodeLoneZeroAsSingleZeroByte()
  {
    Assert.True(Base62.TryDecode("0", out byte[] bytes));
    Assert.Equal(new byte[] { 0 }, bytes);
  }

  [Fact(DisplayName = "Drop leading zero bytes while encoding.")]
  public void DropLeadingZeroBytesWhileEncoding()
  {
    Assert.Equal("1", Base62.Encode([0, 1]));
    Assert.Equal("0", Base62.Encode([0]));
    Assert.Equal("0", Base62.Encode("\0\0"u8));
  }

  [Theory(DisplayName = "Round-trip random bytes through encode and decode.")]
  [InlineData(1)]
  [InlineData(2)]
  [InlineData(3)]
  public void RoundTripRandomBytes(int seed)
  {
    Random random = new(seed);

    for (int length = 1; length <= 256; ++length)
    {
      byte[] bytes = new byte[length];
      random.NextBytes(bytes);

      // A leading zero byte cannot survive a round-trip by design.
      bytes[0] |= 1;

      string encoded = Base62.Encode(bytes);

      Assert.True(Base62.TryDecode(encoded, out byte[] decoded));
      Assert.Equal(bytes, decoded);
    }
  }
}
