// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright © 2022-2026 Aman Agnihotri

namespace Branca.Tests;

public sealed class MockTimer(uint fixedTime) : ITimer
{
  public uint UnixNow { get; } = fixedTime;
}
