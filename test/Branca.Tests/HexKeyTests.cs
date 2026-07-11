// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright © 2022-2026 Aman Agnihotri

namespace Branca.Tests;

using Xunit;
using static TestData;

public sealed class HexKeyTests
{
  [Theory(DisplayName = "Valid hex keys get parsed successfully.")]
  [InlineData(ValidKey01)]
  [InlineData(ValidKey02)]
  public void ValidHexKeysGetParsedSuccessfully(string value)
  {
    HexKey key = value;

    Assert.Equal(32, key.Bytes.Length);
  }

  [Theory(DisplayName = "Invalid hex keys throw ArgumentException.")]
  [InlineData(InvalidKey01)] // Short key
  [InlineData(InvalidKey02)] // Contains non-hexadecimal characters
  public void InvalidHexKeysThrowArgumentException(string value)
  {
    Assert.Throws<ArgumentException>(() =>
    {
      HexKey _ = value;
    });
  }
}
