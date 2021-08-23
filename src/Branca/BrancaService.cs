// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright © 2021 Aman Agnihotri

namespace Branca
{
  using NaCl.Core;
  using System;
  using System.Buffers.Binary;
  using System.Security.Cryptography;
  using System.Text;

  public sealed class BrancaService
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

    public BrancaService(HexKey hexKey) : this(hexKey, new()) { }

    public BrancaService(HexKey hexKey, BrancaSettings settings) :
      this(hexKey.Bytes, settings) { }

    public BrancaService(ReadOnlyMemory<byte> key) : this(key, new()) { }

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
      Span<byte> version = stackalloc byte[VersionLength] {Version};

      Span<byte> timestamp = stackalloc byte[TimeLength];
      BinaryPrimitives.WriteUInt32BigEndian(timestamp, createTime);

      Span<byte> header = stackalloc byte[HeaderLength];
      version.CopyTo(header[..VersionLength]);
      timestamp.CopyTo(header.Slice(VersionLength, TimeLength));
      nonce.CopyTo(header[(VersionLength + TimeLength)..]);

      Span<byte> cipher = payload.Length <= _maxStackLimit
        ? stackalloc byte[payload.Length]
        : new byte[payload.Length];

      Span<byte> tag = stackalloc byte[TagLength];

      _algorithm.Encrypt(nonce, payload, cipher, tag, header);

      int tokenLength = HeaderLength + cipher.Length + tag.Length;

      Span<byte> token = tokenLength <= _maxStackLimit
        ? stackalloc byte[tokenLength]
        : new byte[tokenLength];

      header.CopyTo(token[..HeaderLength]);
      cipher.CopyTo(token.Slice(HeaderLength, cipher.Length));
      tag.CopyTo(token[(HeaderLength + cipher.Length)..]);

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
      ReadOnlySpan<byte> data = Base62.Decode(token);

      Span<byte> version = stackalloc byte[VersionLength];
      data[..VersionLength].CopyTo(version);

      if (version.IsEmpty || version[0] != Version)
      {
        payload = Array.Empty<byte>();
        createTime = default;

        return false;
      }

      Span<byte> timestamp = stackalloc byte[TimeLength];
      data.Slice(VersionLength, TimeLength).CopyTo(timestamp);

      uint creationTime = BinaryPrimitives.ReadUInt32BigEndian(timestamp);

      if (_lifetime.HasValue && _lifetime < _timer.UnixNow - creationTime)
      {
        payload = Array.Empty<byte>();
        createTime = default;

        return false;
      }

      Span<byte> header = stackalloc byte[HeaderLength];
      data[..HeaderLength].CopyTo(header);

      Span<byte> nonce = stackalloc byte[NonceLength];
      header[(VersionLength + TimeLength)..].CopyTo(nonce);

      int cipherLength = data.Length - HeaderLength - TagLength;

      Span<byte> cipher = cipherLength <= _maxStackLimit
        ? stackalloc byte[cipherLength]
        : new byte[cipherLength];

      data.Slice(HeaderLength, cipherLength).CopyTo(cipher);

      Span<byte> tag = stackalloc byte[TagLength];
      data[(HeaderLength + cipherLength)..].CopyTo(tag);

      try
      {
        payload = new byte[cipherLength];

        _algorithm.Decrypt(nonce, cipher, tag, payload, header);

        createTime = creationTime;

        return true;
      }
      catch (Exception)
      {
        payload = Array.Empty<byte>();
        createTime = default;

        return false;
      }
    }
  }
}
