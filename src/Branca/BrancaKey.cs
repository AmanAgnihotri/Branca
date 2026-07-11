// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright © 2026 Aman Agnihotri

namespace Branca;

using System.Security.Cryptography;

public sealed class BrancaKey
{
  private const int KeyLength = 32;

  private readonly byte[] _bytes;

  private BrancaKey(byte[] bytes)
  {
    _bytes = bytes;
  }

  public ReadOnlyMemory<byte> Bytes => _bytes;

  public static BrancaKey Generate()
  {
    byte[] bytes = new byte[KeyLength];

    RandomNumberGenerator.Fill(bytes);

    return new BrancaKey(bytes);
  }

  public static BrancaKey FromBytes(ReadOnlySpan<byte> bytes)
  {
    return new BrancaKey(Validated(bytes.ToArray(), nameof(bytes)));
  }

  public static BrancaKey FromHex(string? value)
  {
    byte[]? bytes = value.AsBytesFromHexString().ToArray();

    return new BrancaKey(Validated(bytes, nameof(value)));
  }

  public static BrancaKey FromBase64Url(string? value)
  {
    if (value is null)
    {
      throw KeyLengthException(nameof(value));
    }

    string base64 = value.Replace('-', '+').Replace('_', '/');

    base64 += (base64.Length % 4) switch
    {
      2 => "==",
      3 => "=",
      0 => "",
      _ => throw FormatException(nameof(value)),
    };

    byte[] bytes;

    try
    {
      bytes = Convert.FromBase64String(base64);
    }
    catch (FormatException)
    {
      throw FormatException(nameof(value));
    }

    return new BrancaKey(Validated(bytes, nameof(value)));
  }

  public static BrancaKey FromBase62(string? value)
  {
    if (!Base62.TryDecode(value, out byte[] bytes) ||
        bytes.Length is 0 or > KeyLength)
    {
      throw FormatException(nameof(value));
    }

    // Base62 drops leading zero bytes, so restore them by right-aligning.
    byte[] key = new byte[KeyLength];

    bytes.CopyTo(key, KeyLength - bytes.Length);

    return new BrancaKey(key);
  }

  public string ToHex()
  {
    return ((ReadOnlySpan<byte>)_bytes).AsHexString();
  }

  public string ToBase64Url()
  {
    return Convert.ToBase64String(_bytes)
      .TrimEnd('=')
      .Replace('+', '-')
      .Replace('/', '_');
  }

  public string ToBase62()
  {
    return Base62.Encode(_bytes);
  }

  public override string ToString()
  {
    return nameof(BrancaKey);
  }

  private static byte[] Validated(byte[] bytes, string parameter)
  {
    return bytes.Length == KeyLength
      ? bytes
      : throw KeyLengthException(parameter);
  }

  private static ArgumentException KeyLengthException(string parameter)
  {
    return new ArgumentException(
      $"Key must be exactly {KeyLength} bytes.", parameter);
  }

  private static ArgumentException FormatException(string parameter)
  {
    return new ArgumentException(
      $"Key must encode exactly {KeyLength} bytes.", parameter);
  }
}
