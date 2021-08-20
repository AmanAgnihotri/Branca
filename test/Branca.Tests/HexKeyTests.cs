// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright © 2021 Aman Agnihotri

namespace Branca.Tests
{
  using System;
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

      Assert.True(key.Bytes.Length == 32);
    }

    [Theory(DisplayName = "Invalid hex keys throw ArgumentException.")]
    [InlineData(InvalidKey01)]
    [InlineData(InvalidKey02)]
    public void InvalidHexKeysThrowArgumentException(string value)
    {
      Assert.Throws<ArgumentException>(() =>
      {
        HexKey _ = value;
      });
    }
  }
}
