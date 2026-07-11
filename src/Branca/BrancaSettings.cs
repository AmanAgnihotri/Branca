// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright © 2022-2026 Aman Agnihotri

namespace Branca;

public sealed record BrancaSettings
{
  public uint MaxStackLimit { get; init; } = 1024;

  public uint? TokenLifetimeInSeconds { get; init; } = 3600;

  public uint ClockSkewInSeconds { get; init; }

  public IReadOnlyList<BrancaKey> PreviousKeys { get; init; } = [];

  public ITimer Timer { get; init; } = new Timer();
}
