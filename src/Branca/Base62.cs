// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright © 2022 Aman Agnihotri

namespace Branca;

internal sealed class Base62
{
  private const string CharacterSet =
    "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";

  private static readonly byte[] LookupTable;

  static Base62()
  {
    LookupTable = new byte[128];

    for (int i = 0; i < CharacterSet.Length; ++i)
    {
      LookupTable[CharacterSet[i]] = (byte)(i & 0xFF);
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

  public static ReadOnlySpan<byte> Decode(string data)
  {
    byte[] bytes = new byte[data.Length];

    for (int i = 0; i < data.Length; i++)
    {
      bytes[i] = LookupTable[data[i]];
    }

    int size = (int)Math.Floor(Math.Log(62) / Math.Log(256) * bytes.Length);

    Stack<byte> result = new(size);
    List<byte> quotients = new(bytes.Length);

    while (bytes.Length > 0)
    {
      quotients.Clear();
      int remainder = 0;

      foreach (byte value in bytes)
      {
        int accumulator = value + (remainder * 62);
        int quotient = Math.DivRem(accumulator, 256, out remainder);

        if (quotients.Count > 0 || quotient != 0)
        {
          quotients.Add((byte)quotient);
        }
      }

      result.Push((byte)remainder);
      bytes = quotients.ToArray();
    }

    return result.ToArray();
  }
}
