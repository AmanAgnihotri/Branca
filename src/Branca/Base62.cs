// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright © 2022-2026 Aman Agnihotri

namespace Branca;

internal static class Base62
{
  private const string CharacterSet =
    "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";

  private const byte InvalidCharacter = 0xFF;

  private static readonly byte[] _lookupTable = CreateLookupTable();

  public static string Encode(ReadOnlySpan<byte> bytes)
  {
    int offset = 0;

    while (offset < bytes.Length && bytes[offset] == 0)
    {
      ++offset;
    }

    if (offset == bytes.Length)
    {
      return bytes.IsEmpty ? string.Empty : "0";
    }

    byte[] number = bytes[offset..].ToArray();

    // Base62 never needs more characters than base16 would use.
    char[] encoded = new char[number.Length * 2];

    int position = encoded.Length;
    int start = 0;

    while (start < number.Length)
    {
      int remainder = 0;

      for (int i = start; i < number.Length; ++i)
      {
        int accumulator = (remainder << 8) | number[i];
        number[i] = (byte)(accumulator / 62);
        remainder = accumulator % 62;
      }

      encoded[--position] = CharacterSet[remainder];

      while (start < number.Length && number[start] == 0)
      {
        ++start;
      }
    }

    return new string(encoded, position, encoded.Length - position);
  }

  public static bool TryDecode(string? data, out byte[] bytes)
  {
    bytes = [];

    if (data is null)
    {
      return false;
    }

    if (data.Length == 0)
    {
      return true;
    }

    if (data.Length > 1 && data[0] == '0')
    {
      return false;
    }

    byte[] digits = new byte[data.Length];

    for (int i = 0; i < data.Length; ++i)
    {
      char character = data[i];

      byte value = character < 128
        ? _lookupTable[character]
        : InvalidCharacter;

      if (value == InvalidCharacter)
      {
        return false;
      }

      digits[i] = value;
    }

    if (digits[0] == 0)
    {
      bytes = [0];

      return true;
    }

    // Base62 always needs at least as many characters as bytes it holds.
    byte[] decoded = new byte[digits.Length];

    int position = decoded.Length;
    int start = 0;

    while (start < digits.Length)
    {
      int remainder = 0;

      for (int i = start; i < digits.Length; ++i)
      {
        int accumulator = (remainder * 62) + digits[i];
        digits[i] = (byte)(accumulator >> 8);
        remainder = accumulator & 0xFF;
      }

      decoded[--position] = (byte)remainder;

      while (start < digits.Length && digits[start] == 0)
      {
        ++start;
      }
    }

    bytes = decoded[position..];

    return true;
  }

  private static byte[] CreateLookupTable()
  {
    byte[] table = new byte[128];

    Array.Fill(table, InvalidCharacter);

    for (int i = 0; i < CharacterSet.Length; ++i)
    {
      table[CharacterSet[i]] = (byte)i;
    }

    return table;
  }
}
