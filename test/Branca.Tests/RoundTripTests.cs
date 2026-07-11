// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright © 2026 Aman Agnihotri

namespace Branca.Tests;

using Xunit;
using static TestData;

public sealed class RoundTripTests
{
  [Theory(DisplayName = "Round-trip payloads of various sizes.")]
  [InlineData(0)]
  [InlineData(1)]
  [InlineData(64)]
  [InlineData(512)]
  [InlineData(1024)]
  [InlineData(4096)]
  public void RoundTripPayloadsOfVariousSizes(int size)
  {
    BrancaService branca = BrancaFactory.Create(Key, Nov2773);

    byte[] payload = new byte[size];
    new Random(size).NextBytes(payload);

    string token = branca.Encode(payload, Nov2773);

    Assert.True(branca.TryDecode(token, out byte[] decoded, out uint time));
    Assert.Equal(payload, decoded);
    Assert.StrictEqual(Nov2773, time);
  }

  [Fact(DisplayName = "Round-trip payloads concurrently on one instance.")]
  public void RoundTripPayloadsConcurrentlyOnOneInstance()
  {
    BrancaService branca = BrancaFactory.Create(Key, Nov2773);

    Parallel.For(0, 1000, index =>
    {
      byte[] payload = BitConverter.GetBytes(index);

      string token = branca.Encode(payload, Nov2773);

      Assert.True(branca.TryDecode(token, out byte[] decoded, out _));
      Assert.Equal(payload, decoded);
    });
  }
}
