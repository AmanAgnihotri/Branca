// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright © 2026 Aman Agnihotri

namespace Branca.Tests;

using Xunit;
using static TestData;

public sealed class ClockSkewTests
{
  private const uint Lifetime = 3600;
  private const uint Skew = 300;

  [Fact(DisplayName = "Future tokens are rejected by default.")]
  public void FutureTokensAreRejectedByDefault()
  {
    using BrancaService branca = Create(Nov2773, 0);

    string token = branca.Encode("Hello, World!", Nov2773 + 1);

    Assert.False(branca.TryDecode(token, out _));
  }

  [Fact(DisplayName = "Future tokens within the skew are accepted.")]
  public void FutureTokensWithinTheSkewAreAccepted()
  {
    using BrancaService branca = Create(Nov2773, Skew);

    string token = branca.Encode("Hello, World!", Nov2773 + Skew);

    Assert.True(branca.TryDecode(token, out _));
  }

  [Fact(DisplayName = "Future tokens beyond the skew are rejected.")]
  public void FutureTokensBeyondTheSkewAreRejected()
  {
    using BrancaService branca = Create(Nov2773, Skew);

    string token = branca.Encode("Hello, World!", Nov2773 + Skew + 1);

    Assert.False(branca.TryDecode(token, out _));
  }

  [Fact(DisplayName = "The skew extends the token lifetime.")]
  public void TheSkewExtendsTheTokenLifetime()
  {
    using BrancaService branca = Create(Nov2773 + Lifetime + Skew, Skew);

    string token = branca.Encode("Hello, World!", Nov2773);

    Assert.True(branca.TryDecode(token, out _));
  }

  [Fact(DisplayName = "Tokens beyond the lifetime and skew are rejected.")]
  public void TokensBeyondTheLifetimeAndSkewAreRejected()
  {
    using BrancaService branca = Create(Nov2773 + Lifetime + Skew + 1, Skew);

    string token = branca.Encode("Hello, World!", Nov2773);

    Assert.False(branca.TryDecode(token, out _));
  }

  [Fact(DisplayName = "Without a lifetime there is no time validation.")]
  public void WithoutALifetimeThereIsNoTimeValidation()
  {
    using BrancaService branca = new(BrancaKey.FromHex(Key), new BrancaSettings
    {
      Timer = new MockTimer(MinTime), TokenLifetimeInSeconds = null,
    });

    string token = branca.Encode("Hello, World!", MaxTime);

    Assert.True(branca.TryDecode(token, out _));
  }

  private static BrancaService Create(uint now, uint skew)
  {
    return new BrancaService(BrancaKey.FromHex(Key), new BrancaSettings
    {
      Timer = new MockTimer(now),
      TokenLifetimeInSeconds = Lifetime,
      ClockSkewInSeconds = skew,
    });
  }
}
