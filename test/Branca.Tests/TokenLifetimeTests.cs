// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright © 2026 Aman Agnihotri

namespace Branca.Tests;

using Xunit;
using static TestData;

public sealed class TokenLifetimeTests
{
  private const uint Lifetime = 3600;

  [Fact(DisplayName = "Decode tokens within their lifetime.")]
  public void DecodeTokensWithinTheirLifetime()
  {
    BrancaService branca = Create(Nov2773 + Lifetime);

    Assert.True(branca.TryDecode(Token03, out _));
  }

  [Fact(DisplayName = "Fail to decode tokens past their lifetime.")]
  public void FailToDecodeTokensPastTheirLifetime()
  {
    BrancaService branca = Create(Nov2773 + Lifetime + 1);

    Assert.False(branca.TryDecode(Token03, out _));
  }

  [Fact(DisplayName = "Decode arbitrarily old tokens without a lifetime.")]
  public void DecodeArbitrarilyOldTokensWithoutLifetime()
  {
    BrancaService branca = new(BrancaKey.FromHex(Key), new BrancaSettings
    {
      Timer = new MockTimer(uint.MaxValue), TokenLifetimeInSeconds = null,
    });

    Assert.True(branca.TryDecode(Token03, out _));
  }

  private static BrancaService Create(uint now)
  {
    return new BrancaService(BrancaKey.FromHex(Key), new BrancaSettings
    {
      Timer = new MockTimer(now), TokenLifetimeInSeconds = Lifetime,
    });
  }
}
