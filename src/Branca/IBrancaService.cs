// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright © 2022 Aman Agnihotri

namespace Branca;

public interface IBrancaService
{
  string Encode(string payload);

  string Encode(string payload, uint createTime);

  string Encode(ReadOnlySpan<byte> payload);

  string Encode(ReadOnlySpan<byte> payload, uint createTime);

  bool TryDecode(string token, out byte[] payload);

  bool TryDecode(string token, out byte[] payload, out uint createTime);
}
