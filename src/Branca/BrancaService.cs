// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright © 2022-2026 Aman Agnihotri

namespace Branca;

using NaCl.Core;
using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text;

public sealed class BrancaService : IBrancaService, IDisposable
{
  private const int KeyLength = 32;
  private const int VersionLength = 1;
  private const int TimeLength = 4;
  private const int NonceLength = 24;
  private const int HeaderLength = VersionLength + TimeLength + NonceLength;
  private const int TagLength = 16;

  private const byte Version = 0xBA;

  private readonly uint _maxStackLimit;
  private readonly uint? _lifetime;

  private readonly ITimer _timer;
  private readonly XChaCha20Poly1305 _algorithm;
  private readonly XChaCha20Poly1305[] _previousAlgorithms;

  public BrancaService(BrancaKey key)
    : this(key, new BrancaSettings()) { }

  public BrancaService(BrancaKey key, BrancaSettings settings)
    : this(key.Bytes, settings) { }

  [Obsolete("Use the BrancaKey overload instead.")]
  public BrancaService(HexKey hexKey)
    : this(hexKey, new BrancaSettings()) { }

  [Obsolete("Use the BrancaKey overload instead.")]
  public BrancaService(HexKey hexKey, BrancaSettings settings)
    : this(hexKey.Bytes, settings) { }

  public BrancaService(ReadOnlyMemory<byte> key)
    : this(key, new BrancaSettings()) { }

  public BrancaService(ReadOnlyMemory<byte> key, BrancaSettings settings)
  {
    if (key.Length != KeyLength)
    {
      string message = $"Key must be exactly {KeyLength} bytes.";

      throw new ArgumentException(message, nameof(key));
    }

    _maxStackLimit = settings.MaxStackLimit;
    _lifetime = settings.TokenLifetimeInSeconds;

    _timer = settings.Timer;
    _algorithm = new XChaCha20Poly1305(key);
    _previousAlgorithms = CreatePreviousAlgorithms(settings.PreviousKeys);
  }

  public void Dispose()
  {
    _algorithm.Dispose();

    foreach (XChaCha20Poly1305 algorithm in _previousAlgorithms)
    {
      algorithm.Dispose();
    }
  }

  public string Encode(string payload)
  {
    return Encode(payload, _timer.UnixNow);
  }

  public string Encode(string payload, uint createTime)
  {
    return Encode(Encoding.UTF8.GetBytes(payload), createTime);
  }

  public string Encode(ReadOnlySpan<byte> payload)
  {
    return Encode(payload, _timer.UnixNow);
  }

  public string Encode(ReadOnlySpan<byte> payload, uint createTime)
  {
    Span<byte> nonce = stackalloc byte[NonceLength];
    RandomNumberGenerator.Fill(nonce);

    return Encode(payload, createTime, nonce);
  }

  internal string Encode(
    ReadOnlySpan<byte> payload,
    uint createTime,
    ReadOnlySpan<byte> nonce)
  {
    int tokenLength = HeaderLength + payload.Length + TagLength;

    Span<byte> token = tokenLength <= _maxStackLimit
      ? stackalloc byte[tokenLength]
      : new byte[tokenLength];

    Span<byte> header = token[..HeaderLength];

    header[0] = Version;

    BinaryPrimitives.WriteUInt32BigEndian(
      header.Slice(VersionLength, TimeLength), createTime);

    nonce.CopyTo(header[(VersionLength + TimeLength)..]);

    Span<byte> cipher = token.Slice(HeaderLength, payload.Length);
    Span<byte> tag = token[(HeaderLength + payload.Length)..];

    _algorithm.Encrypt(nonce, payload, cipher, tag, header);

    return Base62.Encode(token);
  }

  public bool TryDecode(string token, out byte[] payload)
  {
    return TryDecode(token, out payload, out _);
  }

  public bool TryDecode(
    string token,
    out byte[] payload,
    out uint createTime)
  {
    payload = [];
    createTime = 0;

    if (!Base62.TryDecode(token, out byte[] decoded))
    {
      return false;
    }

    ReadOnlySpan<byte> data = decoded;

    if (data.Length < HeaderLength + TagLength)
    {
      return false;
    }

    if (data[0] != Version)
    {
      return false;
    }

    ReadOnlySpan<byte> timestamp = data.Slice(VersionLength, TimeLength);

    uint creationTime = BinaryPrimitives.ReadUInt32BigEndian(timestamp);

    if (_lifetime.HasValue && _lifetime < _timer.UnixNow - creationTime)
    {
      return false;
    }

    ReadOnlySpan<byte> header = data[..HeaderLength];
    ReadOnlySpan<byte> nonce = header[(VersionLength + TimeLength)..];

    int cipherLength = data.Length - HeaderLength - TagLength;

    ReadOnlySpan<byte> cipher = data.Slice(HeaderLength, cipherLength);
    ReadOnlySpan<byte> tag = data[(HeaderLength + cipherLength)..];

    byte[] plaintext = new byte[cipherLength];

    if (!TryDecrypt(_algorithm, nonce, cipher, tag, plaintext, header) &&
        !TryDecryptWithPreviousKeys(nonce, cipher, tag, plaintext, header))
    {
      return false;
    }

    payload = plaintext;
    createTime = creationTime;

    return true;
  }

  private bool TryDecryptWithPreviousKeys(
    ReadOnlySpan<byte> nonce,
    ReadOnlySpan<byte> cipher,
    ReadOnlySpan<byte> tag,
    byte[] plaintext,
    ReadOnlySpan<byte> header)
  {
    foreach (XChaCha20Poly1305 algorithm in _previousAlgorithms)
    {
      if (TryDecrypt(algorithm, nonce, cipher, tag, plaintext, header))
      {
        return true;
      }
    }

    return false;
  }

  private static bool TryDecrypt(
    XChaCha20Poly1305 algorithm,
    ReadOnlySpan<byte> nonce,
    ReadOnlySpan<byte> cipher,
    ReadOnlySpan<byte> tag,
    byte[] plaintext,
    ReadOnlySpan<byte> header)
  {
    try
    {
      algorithm.Decrypt(nonce, cipher, tag, plaintext, header);

      return true;
    }
    catch (CryptographicException)
    {
      return false;
    }
  }

  private static XChaCha20Poly1305[] CreatePreviousAlgorithms(
    IReadOnlyList<BrancaKey> keys)
  {
    if (keys.Count == 0)
    {
      return [];
    }

    XChaCha20Poly1305[] algorithms = new XChaCha20Poly1305[keys.Count];

    for (int i = 0; i < keys.Count; ++i)
    {
      algorithms[i] = new XChaCha20Poly1305(keys[i].Bytes);
    }

    return algorithms;
  }
}
