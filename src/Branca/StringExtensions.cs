// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright © 2021 Aman Agnihotri

namespace Branca
{
  using System;
  using System.Linq;

  internal static class StringExtensions
  {
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

        static int Hex(char hex) =>
          hex - (hex <= '9' ? '0' : hex < 'a' ? '7' : 'W');
      }

      return bytes;

      static bool IsNotHexCharacter(char c) =>
        c is (< '0' or > '9') and (< 'a' or > 'f') and (< 'A' or > 'F');
    }
  }
}
