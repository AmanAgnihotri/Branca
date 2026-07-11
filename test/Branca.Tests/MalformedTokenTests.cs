// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright © 2026 Aman Agnihotri

namespace Branca.Tests;

using Xunit;
using static TestData;

public sealed class MalformedTokenTests
{
  // Version, timestamp, nonce and tag lengths per the Branca specification.
  private const int MinTokenLength = 1 + 4 + 24 + 16;

  private const byte Version = 0xBA;

  [Fact(DisplayName = "Fail to decode null and empty tokens.")]
  public void FailToDecodeNullAndEmptyTokens()
  {
    BrancaService branca = BrancaFactory.Create(Key, MinTime);

    Assert.False(branca.TryDecode(null!, out _));
    Assert.False(branca.TryDecode(string.Empty, out _));
  }

  [Fact(DisplayName = "Fail to decode every truncation of a valid token.")]
  public void FailToDecodeTruncatedTokens()
  {
    BrancaService branca = BrancaFactory.Create(Key, MinTime);

    for (int length = 1; length < Token01.Length; ++length)
    {
      Assert.False(branca.TryDecode(Token01[..length], out _));
    }
  }

  [Fact(DisplayName = "Fail to decode tokens with too few bytes.")]
  public void FailToDecodeTokensWithTooFewBytes()
  {
    BrancaService branca = BrancaFactory.Create(Key, MinTime);

    for (int length = 1; length < MinTokenLength; ++length)
    {
      byte[] data = new byte[length];
      data[0] = Version;

      string token = Base62.Encode(data);

      Assert.False(branca.TryDecode(token, out _));
    }
  }

  [Fact(DisplayName = "Fail to decode tokens with invalid characters.")]
  public void FailToDecodeTokensWithInvalidCharacters()
  {
    BrancaService branca = BrancaFactory.Create(Key, MinTime);

    Assert.False(branca.TryDecode(Token01.Replace('0', '_'), out _));
    Assert.False(branca.TryDecode("_" + Token01, out _));
    Assert.False(branca.TryDecode(Token01 + "!", out _));
    Assert.False(branca.TryDecode("♥" + Token01, out _));
    Assert.False(branca.TryDecode(Token01.Insert(3, " "), out _));
  }

  [Fact(DisplayName = "Fail to decode non-canonical tokens.")]
  public void FailToDecodeNonCanonicalTokens()
  {
    BrancaService branca = BrancaFactory.Create(Key, MinTime);

    Assert.False(branca.TryDecode("0" + Token01, out _));
    Assert.False(branca.TryDecode("000" + Token01, out _));
  }

  [Fact(DisplayName = "Reset outputs when decoding fails.")]
  public void ResetOutputsWhenDecodingFails()
  {
    BrancaService branca = BrancaFactory.Create(Key, MinTime);

    bool isValid = branca.TryDecode(
      "0" + Token01, out byte[] payload, out uint createTime);

    Assert.False(isValid);
    Assert.True(payload == Array.Empty<byte>());
    Assert.Equal(0U, createTime);
  }
}
