// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright © 2022-2026 Aman Agnihotri

namespace Branca;

[Obsolete("Use BrancaKey and its factory methods, such as BrancaKey.FromHex.")]
public sealed record HexKey
{
  public ReadOnlyMemory<byte> Bytes { get; }

  public HexKey(string? hexKey)
  {
    Bytes = BrancaKey.FromHex(hexKey).Bytes;
  }

  public static implicit operator HexKey(string hexKey)
  {
    return new HexKey(hexKey);
  }
}
