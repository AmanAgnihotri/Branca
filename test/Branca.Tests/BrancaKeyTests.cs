// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright © 2026 Aman Agnihotri

namespace Branca.Tests;

using Xunit;
using static TestData;

public sealed class BrancaKeyTests
{
  [Fact(DisplayName = "Generate produces distinct 32-byte keys.")]
  public void GenerateProducesDistinctKeys()
  {
    BrancaKey first = BrancaKey.Generate();
    BrancaKey second = BrancaKey.Generate();

    Assert.Equal(32, first.Bytes.Length);
    Assert.Equal(32, second.Bytes.Length);
    Assert.False(first.Bytes.Span.SequenceEqual(second.Bytes.Span));
  }

  [Fact(DisplayName = "Round-trip a key through every encoding.")]
  public void RoundTripKeyThroughEveryEncoding()
  {
    BrancaKey key = BrancaKey.FromHex(ValidKey01);

    Assert.Equal(
      key.Bytes.ToArray(), BrancaKey.FromHex(key.ToHex()).Bytes.ToArray());

    Assert.Equal(
      key.Bytes.ToArray(),
      BrancaKey.FromBase64Url(key.ToBase64Url()).Bytes.ToArray());

    Assert.Equal(
      key.Bytes.ToArray(), BrancaKey.FromBase62(key.ToBase62()).Bytes.ToArray());
  }

  [Fact(DisplayName = "Every encoding of a key decodes to the same bytes.")]
  public void EveryEncodingDecodesToTheSameBytes()
  {
    byte[] expected = BrancaKey.FromHex(ValidKey01).Bytes.ToArray();

    BrancaKey key = BrancaKey.FromHex(ValidKey01);

    Assert.Equal(expected, BrancaKey.FromBase64Url(key.ToBase64Url()).Bytes.ToArray());
    Assert.Equal(expected, BrancaKey.FromBase62(key.ToBase62()).Bytes.ToArray());
    Assert.Equal(expected, BrancaKey.FromBytes(key.Bytes.Span).Bytes.ToArray());
  }

  [Fact(DisplayName = "Base64Url renders 43 unpadded URL-safe characters.")]
  public void Base64UrlRendersUnpaddedUrlSafeCharacters()
  {
    string value = BrancaKey.FromHex(ValidKey01).ToBase64Url();

    Assert.Equal(43, value.Length);
    Assert.DoesNotContain('=', value);
    Assert.DoesNotContain('+', value);
    Assert.DoesNotContain('/', value);
  }

  [Fact(DisplayName = "Hex renders 64 lowercase characters.")]
  public void HexRendersLowercaseCharacters()
  {
    Assert.Equal(ValidKey01, BrancaKey.FromHex(ValidKey01).ToHex());
  }

  [Fact(DisplayName = "Keys with leading zero bytes survive Base62.")]
  public void KeysWithLeadingZeroBytesSurviveBase62()
  {
    byte[] bytes = new byte[32];
    bytes[31] = 1;

    BrancaKey key = BrancaKey.FromBytes(bytes);

    Assert.Equal(bytes, BrancaKey.FromBase62(key.ToBase62()).Bytes.ToArray());
  }

  [Fact(DisplayName = "All encodings drive an equivalent BrancaService.")]
  public void AllEncodingsDriveAnEquivalentService()
  {
    BrancaKey key = BrancaKey.FromHex(ValidKey01);

    string token = new BrancaService(key).Encode("Hello, World!");

    foreach (BrancaKey other in new[]
    {
      BrancaKey.FromBase64Url(key.ToBase64Url()),
      BrancaKey.FromBase62(key.ToBase62()),
      BrancaKey.FromBytes(key.Bytes.Span),
    })
    {
      Assert.True(new BrancaService(other).TryDecode(token, out _));
    }
  }

  [Fact(DisplayName = "FromBytes copies its input defensively.")]
  public void FromBytesCopiesItsInput()
  {
    byte[] bytes = BrancaKey.FromHex(ValidKey01).Bytes.ToArray();

    BrancaKey key = BrancaKey.FromBytes(bytes);

    Array.Clear(bytes, 0, bytes.Length);

    Assert.NotEqual(bytes, key.Bytes.ToArray());
  }

  [Fact(DisplayName = "ToString does not reveal the key material.")]
  public void ToStringDoesNotRevealKeyMaterial()
  {
    Assert.Equal(nameof(BrancaKey), BrancaKey.FromHex(ValidKey01).ToString());
  }

  [Theory(DisplayName = "Invalid hex values throw ArgumentException.")]
  [InlineData(null)]
  [InlineData("")]
  [InlineData(InvalidKey01)]
  [InlineData(InvalidKey02)]
  public void InvalidHexValuesThrow(string? value)
  {
    Assert.Throws<ArgumentException>(() => BrancaKey.FromHex(value));
  }

  [Theory(DisplayName = "Invalid Base64Url values throw ArgumentException.")]
  [InlineData(null)]
  [InlineData("")]
  [InlineData("short")]
  [InlineData("!!!!")]
  public void InvalidBase64UrlValuesThrow(string? value)
  {
    Assert.Throws<ArgumentException>(() => BrancaKey.FromBase64Url(value));
  }

  [Theory(DisplayName = "Invalid Base62 values throw ArgumentException.")]
  [InlineData(null)]
  [InlineData("")]
  [InlineData("_")]
  public void InvalidBase62ValuesThrow(string? value)
  {
    Assert.Throws<ArgumentException>(() => BrancaKey.FromBase62(value));
  }

  [Fact(DisplayName = "Base62 values longer than 32 bytes throw.")]
  public void OversizedBase62ValuesThrow()
  {
    byte[] bytes = new byte[33];
    Array.Fill(bytes, (byte)0xFF);

    string oversized = Base62.Encode(bytes);

    Assert.Throws<ArgumentException>(() => BrancaKey.FromBase62(oversized));
  }

  [Fact(DisplayName = "FromBytes rejects the wrong length.")]
  public void FromBytesRejectsWrongLength()
  {
    Assert.Throws<ArgumentException>(() => BrancaKey.FromBytes(new byte[16]));
  }
}
