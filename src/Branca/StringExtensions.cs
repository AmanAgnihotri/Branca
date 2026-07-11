// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright © 2022-2026 Aman Agnihotri

namespace Branca;

internal static class StringExtensions
{
  private const string HexDigits = "0123456789abcdef";

  public static string AsHexString(this ReadOnlySpan<byte> bytes)
  {
    Span<char> hex = bytes.Length <= 512
      ? stackalloc char[bytes.Length * 2]
      : new char[bytes.Length * 2];

    for (int i = 0; i < bytes.Length; ++i)
    {
      hex[i * 2] = HexDigits[bytes[i] >> 4];
      hex[(i * 2) + 1] = HexDigits[bytes[i] & 0x0F];
    }

    return new string(hex);
  }

  public static ReadOnlyMemory<byte> AsBytesFromHexString(this string? hexKey)
  {
    if (hexKey is null)
    {
      return ReadOnlyMemory<byte>.Empty;
    }

    if (hexKey.Length % 2 != 0)
    {
      throw new ArgumentException("Key must have even number of characters.");
    }

    if (hexKey.Any(IsNotHexCharacter))
    {
      const string message = "Key must only have hexadecimal characters.";

      throw new ArgumentException(message, nameof(hexKey));
    }

    int length = hexKey.Length >> 1;

    byte[] bytes = new byte[length];

    for (int i = 0, j = 0; i < length; ++i, j = i << 1)
    {
      bytes[i] = (byte)((Hex(hexKey[j]) << 4) + Hex(hexKey[j + 1]));

      continue;

      static int Hex(char hex)
      {
        return hex - (hex <= '9' ? '0' : hex < 'a' ? '7' : 'W');
      }
    }

    return bytes;

    static bool IsNotHexCharacter(char c)
    {
      return c is (< '0' or > '9') and (< 'a' or > 'f') and (< 'A' or > 'F');
    }
  }
}
