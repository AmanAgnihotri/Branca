// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright © 2022 Aman Agnihotri

namespace Branca.Tests;

using Xunit;
using static TestData;

public sealed class BrancaTests
{
  [Theory(DisplayName = "Encode and decode valid cases properly.")]
  [MemberData(nameof(ValidTestData))]
  public void EncodeAndDecodeValidCasesProperly(BrancaState data)
  {
    BrancaService branca = BrancaFactory.Create(data.Key, data.Timestamp);

    string token = branca.Encode(
      data.Message.Span, data.Timestamp, data.Nonce.Span);

    Assert.Equal(data.Token, token);

    bool isValid = branca.TryDecode(
      data.Token, out byte[] message, out uint timestamp);

    Assert.StrictEqual(data.IsValid, isValid);

    Assert.True(data.Message.Span.SequenceEqual(message));

    Assert.StrictEqual(data.Timestamp, timestamp);
  }

  [Theory(DisplayName = "Fail to decode in invalid cases.")]
  [MemberData(nameof(InvalidTestData))]
  public void FailToDecodeInvalidCases(BrancaState data)
  {
    BrancaService branca = BrancaFactory.Create(data.Key, data.Timestamp);

    bool isValid = branca.TryDecode(
      data.Token, out byte[] message, out uint timestamp);

    Assert.StrictEqual(data.IsValid, isValid);
    Assert.True(message == Array.Empty<byte>());
    Assert.True(timestamp == default);
  }

  public static readonly TheoryData<BrancaState> ValidTestData = new()
  {
    // Hello world with zero timestamp
    new(Key, Nonce, MinTime, Token01, HelloMessage, true),
    // Hello world with max timestamp
    new(Key, Nonce, MaxTime, Token02, HelloMessage, true),
    // Hello world with November 27 timestamp
    new(Key, Nonce, Nov2773, Token03, HelloMessage, true),
    // Eight null bytes with zero timestamp
    new(Key, Nonce, MinTime, Token04, Null8Message, true),
    // Eight null bytes with max timestamp
    new(Key, Nonce, MaxTime, Token05, Null8Message, true),
    // Eight null bytes with November 27th timestamp
    new(Key, Nonce, Nov2773, Token06, Null8Message, true),
    // Empty payload
    new(Key, Nonce, MinTime, Token07, EmptyMessage, true),
    // Non-UTF8 payload
    new(Key, Nonce, Nov2773, Token08, Hex80Message, true)
  };

  public static readonly TheoryData<BrancaState> InvalidTestData = new()
  {
    // Wrong version 0xBB
    new(Key, NullNonce, MinTime, Token09, EmptyMessage, false),
    // Invalid base62 characters
    new(Key, NullNonce, Nov2773, Token10, HelloMessage, false),
    // Modified version
    new(Key, NullNonce, MinTime, Token11, HelloMessage, false),
    // Modified first byte of the nonce
    new(Key, NullNonce, MinTime, Token12, HelloMessage, false),
    // Modified timestamp
    new(Key, NullNonce, MinTime, Token13, HelloMessage, false),
    // Modified last byte of the ciphertext
    new(Key, NullNonce, MinTime, Token14, HelloMessage, false),
    // Modified last byte of the Poly1305 tag
    new(Key, NullNonce, MinTime, Token15, HelloMessage, false),
    // Wrong key
    new(ValidKey02, NullNonce, MinTime, Token01, HelloMessage, false)
  };
}
