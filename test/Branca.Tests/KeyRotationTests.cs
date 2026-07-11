// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright © 2026 Aman Agnihotri

namespace Branca.Tests;

using System.Text;
using Xunit;
using static TestData;

public sealed class KeyRotationTests
{
  private static readonly BrancaKey _current = BrancaKey.FromHex(ValidKey01);
  private static readonly BrancaKey _previous = BrancaKey.FromHex(ValidKey02);

  [Fact(DisplayName = "Tokens from the current key round-trip.")]
  public void TokensFromTheCurrentKeyRoundTrip()
  {
    using BrancaService rotated = Rotated();

    string token = rotated.Encode("Hello, World!");

    Assert.True(rotated.TryDecode(token, out byte[] payload));
    Assert.Equal("Hello, World!", Encoding.UTF8.GetString(payload));
  }

  [Fact(DisplayName = "Tokens from a previous key still decode.")]
  public void TokensFromAPreviousKeyStillDecode()
  {
    using BrancaService old = new(_previous);
    using BrancaService rotated = Rotated();

    string token = old.Encode("Hello, World!");

    Assert.True(rotated.TryDecode(token, out byte[] payload));
    Assert.Equal("Hello, World!", Encoding.UTF8.GetString(payload));
  }

  [Fact(DisplayName = "Tokens from every listed previous key decode.")]
  public void TokensFromEveryListedPreviousKeyDecode()
  {
    BrancaKey older = BrancaKey.Generate();

    using BrancaService first = new(older);
    using BrancaService second = new(_previous);

    using BrancaService rotated = new(_current, new BrancaSettings
    {
      PreviousKeys = [_previous, older],
    });

    Assert.True(rotated.TryDecode(first.Encode("one"), out _));
    Assert.True(rotated.TryDecode(second.Encode("two"), out _));
  }

  [Fact(DisplayName = "Tokens from an unknown key fail to decode.")]
  public void TokensFromAnUnknownKeyFailToDecode()
  {
    using BrancaService unknown = new(BrancaKey.Generate());
    using BrancaService rotated = Rotated();

    string token = unknown.Encode("Hello, World!");

    Assert.False(rotated.TryDecode(token, out _));
  }

  [Fact(DisplayName = "Encoding uses only the current key.")]
  public void EncodingUsesOnlyTheCurrentKey()
  {
    using BrancaService rotated = Rotated();
    using BrancaService old = new(_previous);

    string token = rotated.Encode("Hello, World!");

    Assert.False(old.TryDecode(token, out _));
  }

  [Fact(DisplayName = "The token lifetime applies to previous keys too.")]
  public void TheTokenLifetimeAppliesToPreviousKeysToo()
  {
    using BrancaService old = new(_previous);

    string token = old.Encode("Hello, World!", Nov2773);

    using BrancaService rotated = new(_current, new BrancaSettings
    {
      PreviousKeys = [_previous],
      Timer = new MockTimer(Nov2773 + 3601),
    });

    Assert.False(rotated.TryDecode(token, out _));
  }

  private static BrancaService Rotated()
  {
    return new BrancaService(_current, new BrancaSettings
    {
      PreviousKeys = [_previous],
    });
  }
}
