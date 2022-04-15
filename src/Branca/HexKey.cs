// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright © 2022 Aman Agnihotri

namespace Branca;

using System;

public sealed record HexKey
{
  private const int KeyLength = 32;
  private const int HexKeyLength = KeyLength * 2;

  public ReadOnlyMemory<byte> Bytes { get; }

  public HexKey(string hexKey)
  {
    if (hexKey is null || hexKey.Length != HexKeyLength)
    {
      string message = $"Key must have exactly {HexKeyLength} characters.";

      throw new ArgumentException(message, nameof(hexKey));
    }

    Bytes = hexKey.AsBytesFromHexString();
  }

  public static implicit operator HexKey(string hexKey)
  {
    return new HexKey(hexKey);
  }
}
