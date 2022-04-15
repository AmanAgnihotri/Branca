// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright © 2022 Aman Agnihotri

namespace Branca.Tests;

public sealed record BrancaState
{
  public HexKey Key { get; }

  public ReadOnlyMemory<byte> Nonce { get; }

  public uint Timestamp { get; }

  public string Token { get; }

  public ReadOnlyMemory<byte> Message { get; }

  public bool IsValid { get; }

  public BrancaState(
    HexKey key,
    string? nonce,
    uint timestamp,
    string token,
    string message,
    bool isValid)
  {
    Key = key;
    Nonce = nonce.AsBytesFromHexString();
    Timestamp = timestamp;
    Token = token;
    Message = message.AsBytesFromHexString();
    IsValid = isValid;
  }
}
