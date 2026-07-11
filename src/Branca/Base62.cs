// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright © 2022-2026 Aman Agnihotri

namespace Branca;

internal sealed class Base62
{
  private const string CharacterSet =
    "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";

  private const byte InvalidCharacter = 0xFF;

  private static readonly byte[] _lookupTable;

  static Base62()
  {
    _lookupTable = new byte[128];

    Array.Fill(_lookupTable, InvalidCharacter);

    for (int i = 0; i < CharacterSet.Length; ++i)
    {
      _lookupTable[CharacterSet[i]] = (byte)(i & 0xFF);
    }
  }

  public static string Encode(ReadOnlySpan<byte> bytes)
  {
    if (bytes.Length == 0)
    {
      return string.Empty;
    }

    int size = (int)Math.Ceiling(Math.Log(256) / Math.Log(62) * bytes.Length);

    Stack<char> result = new(size);
    List<byte> quotients = new(bytes.Length);

    while (bytes.Length > 0)
    {
      quotients.Clear();
      int remainder = 0;

      foreach (byte value in bytes)
      {
        int accumulator = value + (remainder * 256);
        int quotient = Math.DivRem(accumulator, 62, out remainder);

        if (quotients.Count > 0 || quotient != 0)
        {
          quotients.Add((byte)quotient);
        }
      }

      result.Push(CharacterSet[remainder]);
      bytes = quotients.ToArray();
    }

    return new string(result.ToArray());
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

    byte[] values = new byte[data.Length];

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

      values[i] = value;
    }

    int size = (int)Math.Floor(Math.Log(62) / Math.Log(256) * values.Length);

    Stack<byte> result = new(size);
    List<byte> quotients = new(values.Length);

    ReadOnlySpan<byte> digits = values;

    while (digits.Length > 0)
    {
      quotients.Clear();
      int remainder = 0;

      foreach (byte value in digits)
      {
        int accumulator = value + (remainder * 62);
        int quotient = Math.DivRem(accumulator, 256, out remainder);

        if (quotients.Count > 0 || quotient != 0)
        {
          quotients.Add((byte)quotient);
        }
      }

      result.Push((byte)remainder);
      digits = quotients.ToArray();
    }

    bytes = result.ToArray();

    return true;
  }
}
